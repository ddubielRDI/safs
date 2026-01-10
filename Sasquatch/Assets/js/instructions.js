// =============================================================================
// Instructions.js - "Show Instructions" toggle functionality
// =============================================================================
// Displays numbered instruction pills above elements with data-instruction attr
// when the "Show Instructions" checkbox is enabled.

(function() {
    'use strict';

    // Instruction definitions by workflow and view
    const instructions = {
        'Enrollment': {
            'Index': [
                { elementId: 'upload-file', stepNumber: 1, text: 'Upload enrollment data from a CSV file' },
                { elementId: 'manual-entry', stepNumber: 2, text: 'Or enter data manually by school' },
                { elementId: 'view-submission', stepNumber: 3, text: 'View or edit a submission' }
            ],
            'Details': [
                { elementId: 'edit-headcount', stepNumber: 1, text: 'Edit headcount or FTE values directly' },
                { elementId: 'add-comment', stepNumber: 2, text: 'Add explanation to resolve warnings' },
                { elementId: 'submit-approval', stepNumber: 3, text: 'Submit when all errors are resolved' }
            ],
            'Upload': [
                { elementId: 'file-drop', stepNumber: 1, text: 'Click or drag your enrollment file here' },
                { elementId: 'upload-validate', stepNumber: 2, text: 'Click to validate and process' }
            ],
            'ManualEntry': [
                { elementId: 'school-select', stepNumber: 1, text: 'Select a school to edit' },
                { elementId: 'edit-fields', stepNumber: 2, text: 'Enter enrollment counts' },
                { elementId: 'save-data', stepNumber: 3, text: 'Save your changes' }
            ]
        },
        'Budget': {
            'Index': [
                { elementId: 'upload-budget', stepNumber: 1, text: 'Upload a budget file (CSV or Excel)' },
                { elementId: 'view-submission', stepNumber: 2, text: 'View or edit a submission' }
            ],
            'Details': [
                { elementId: 'edit-amount', stepNumber: 1, text: 'Edit revenue or expenditure amounts' },
                { elementId: 'add-comment', stepNumber: 2, text: 'Add explanation for variances' },
                { elementId: 'submit-approval', stepNumber: 3, text: 'Submit when budget is balanced' }
            ],
            'Upload': [
                { elementId: 'budget-type', stepNumber: 1, text: 'Select Original, Revised, or Final' },
                { elementId: 'file-drop', stepNumber: 2, text: 'Click or drag your budget file here' },
                { elementId: 'upload-validate', stepNumber: 3, text: 'Click to validate and upload' }
            ]
        }
    };

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
    function positionPill(pill, element) {
        const buffer = 10; // px from viewport edge
        const viewportWidth = window.innerWidth;

        // Position above element
        pill.style.top = '-2.5rem';

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
                align-items: center;
                gap: 0.5rem;
                padding: 0.35rem 0.75rem;
                background: linear-gradient(135deg, #198754 0%, #20c997 100%);
                color: white;
                border-radius: 50px;
                font-size: 0.85rem;
                font-weight: 500;
                box-shadow: 0 2px 8px rgba(25, 135, 84, 0.4);
                white-space: nowrap;
                pointer-events: none;
                animation: instructionFadeIn 0.3s ease-out;
            }
            .instruction-number {
                display: inline-flex;
                align-items: center;
                justify-content: center;
                width: 1.5rem;
                height: 1.5rem;
                background: rgba(255, 255, 255, 0.25);
                border-radius: 50%;
                font-weight: 700;
            }
            .instruction-text {
                max-width: 250px;
                overflow: hidden;
                text-overflow: ellipsis;
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

        const viewInstructions = instructions[workflow]?.[view] || [];

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
            positionPill(pill, element);
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
                    positionPill(pill, element);
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
