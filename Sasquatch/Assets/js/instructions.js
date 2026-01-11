// =============================================================================
// Instructions.js - "Show Instructions" toggle functionality
// =============================================================================
// Displays numbered instruction pills above elements with data-instruction attr
// when the "Show Instructions" checkbox is enabled.
//
// Instructions are provided by the server via window.workflowInstructions
// (injected from InstructionService.cs through ViewBag.InstructionsJson)

(function() {
    'use strict';

    // Create instruction pill element
    function createPill(step) {
        const pill = document.createElement('div');
        pill.className = 'instruction-pill';
        pill.innerHTML = `
            <span class="instruction-number">${step.stepNumber}</span>
            <span class="instruction-text">${step.text}</span>
        `;
        return pill;
    }

    // Position pill within viewport bounds
    function positionPill(pill, element, stepNumber) {
        const buffer = 10; // px from viewport edge
        const viewportWidth = window.innerWidth;

        // Stagger heights: odd steps at -2.5rem, even steps at -4.5rem
        pill.style.top = stepNumber % 2 === 0 ? '-4.5rem' : '-2.5rem';

        // Pre-check: if element is near right edge, default to right-align
        const elementRect = element.getBoundingClientRect();
        const estimatedPillWidth = 280; // max-width + padding estimate

        if (elementRect.left + estimatedPillWidth > viewportWidth - buffer) {
            pill.style.left = 'auto';
            pill.style.right = '0';
        } else {
            pill.style.left = '0';
            pill.style.right = 'auto';
        }

        // Verify after render and adjust if needed
        requestAnimationFrame(() => {
            const pillRect = pill.getBoundingClientRect();

            // Fix right overflow
            if (pillRect.right > viewportWidth - buffer) {
                pill.style.left = 'auto';
                pill.style.right = '0';
            }
            // Fix left overflow (from very long pills)
            else if (pillRect.left < buffer) {
                pill.style.left = '0';
                pill.style.right = 'auto';
            }
        });
    }

    // Add instruction styles to page
    function addStyles() {
        if (document.getElementById('instruction-styles')) return;

        const styles = document.createElement('style');
        styles.id = 'instruction-styles';
        styles.textContent = `
            .instruction-pill {
                position: absolute;
                z-index: 1050;
                display: inline-flex;
                align-items: flex-start;
                gap: 0.5rem;
                padding: 0.35rem 0.75rem;
                background: linear-gradient(135deg, #198754 0%, #20c997 100%);
                color: white;
                border-radius: 12px;
                font-size: 0.85rem;
                font-weight: 500;
                box-shadow: 0 2px 8px rgba(25, 135, 84, 0.4);
                pointer-events: none;
                animation: instructionFadeIn 0.3s ease-out;
                max-width: 400px;
            }
            .instruction-number {
                display: inline-flex;
                align-items: center;
                justify-content: center;
                min-width: 1.5rem;
                height: 1.5rem;
                background: rgba(255, 255, 255, 0.25);
                border-radius: 50%;
                font-weight: 700;
                flex-shrink: 0;
            }
            .instruction-text {
                line-height: 1.4;
                white-space: normal !important;
                overflow: visible !important;
                text-overflow: clip !important;
                max-width: none !important;
            }
            @keyframes instructionFadeIn {
                from { opacity: 0; transform: translateY(5px); }
                to { opacity: 1; transform: translateY(0); }
            }
            [data-instruction] {
                position: relative;
            }
        `;
        document.head.appendChild(styles);
    }

    // Show instructions for current workflow/view
    function showInstructions(workflow, view) {
        addStyles();

        // Use server-provided instructions (single source of truth)
        const viewInstructions = window.workflowInstructions || [];

        viewInstructions.forEach(step => {
            // Find first matching element (there may be multiple with same data-instruction)
            const element = document.querySelector(`[data-instruction="${step.elementId}"]`);
            if (!element) return;

            // Skip if already has instruction
            if (element.querySelector('.instruction-pill')) return;

            // Make element position relative if needed
            const computed = window.getComputedStyle(element);
            if (computed.position === 'static') {
                element.style.position = 'relative';
            }

            // Create and position pill
            const pill = createPill(step);
            element.appendChild(pill);

            // Position above the element, respecting viewport bounds
            positionPill(pill, element, step.stepNumber);
        });
    }

    // Hide all instructions
    function hideInstructions() {
        document.querySelectorAll('.instruction-pill').forEach(pill => {
            pill.remove();
        });
    }

    // Reposition pills on window resize
    let resizeTimeout;
    function handleResize() {
        clearTimeout(resizeTimeout);
        resizeTimeout = setTimeout(() => {
            document.querySelectorAll('.instruction-pill').forEach(pill => {
                const element = pill.parentElement;
                if (element) {
                    // Extract step number from pill's number element
                    const stepNumber = parseInt(pill.querySelector('.instruction-number')?.textContent) || 1;
                    positionPill(pill, element, stepNumber);
                }
            });
        }, 100);
    }
    window.addEventListener('resize', handleResize);

    // Initialize on DOM ready
    function init() {
        const toggle = document.getElementById('showInstructionsToggle');
        if (!toggle) return;

        const workflow = toggle.dataset.workflow;
        const view = toggle.dataset.view;

        toggle.addEventListener('change', function() {
            if (this.checked) {
                showInstructions(workflow, view);
            } else {
                hideInstructions();
            }
        });

        // Show instructions if toggle is already checked on page load
        if (toggle.checked) {
            showInstructions(workflow, view);
        }
    }

    // Run when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
