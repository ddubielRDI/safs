/**
 * SASQUATCH Theme Switcher
 * Handles theme and dark mode switching with localStorage persistence
 */
(function () {
    'use strict';

    const STORAGE_KEYS = {
        THEME: 'sasquatch-theme',
        MODE: 'sasquatch-mode'
    };

    const DEFAULTS = {
        THEME: 'minimal',
        MODE: 'light'
    };

    // Valid values for validation
    const VALID_THEMES = ['minimal', 'dense', 'friendly', 'glass'];
    const VALID_MODES = ['light', 'dark'];

    /**
     * Safely get item from localStorage with fallback
     * @param {string} key - Storage key
     * @param {string} fallback - Fallback value
     * @returns {string}
     */
    function safeGetStorage(key, fallback) {
        try {
            return localStorage.getItem(key) || fallback;
        } catch (e) {
            console.warn('localStorage unavailable:', e.message);
            return fallback;
        }
    }

    /**
     * Safely set item in localStorage
     * @param {string} key - Storage key
     * @param {string} value - Value to store
     */
    function safeSetStorage(key, value) {
        try {
            localStorage.setItem(key, value);
        } catch (e) {
            console.warn('localStorage unavailable:', e.message);
        }
    }

    /**
     * Validate theme value
     * @param {string} theme - Theme to validate
     * @returns {string} - Valid theme or default
     */
    function validateTheme(theme) {
        return VALID_THEMES.includes(theme) ? theme : DEFAULTS.THEME;
    }

    /**
     * Validate mode value
     * @param {string} mode - Mode to validate
     * @returns {string} - Valid mode or default
     */
    function validateMode(mode) {
        return VALID_MODES.includes(mode) ? mode : DEFAULTS.MODE;
    }

    /**
     * Initialize theme from localStorage or defaults
     * Called immediately to prevent flash of unstyled content
     */
    function initTheme() {
        const savedTheme = validateTheme(safeGetStorage(STORAGE_KEYS.THEME, DEFAULTS.THEME));
        const savedMode = validateMode(safeGetStorage(STORAGE_KEYS.MODE, DEFAULTS.MODE));

        applyTheme(savedTheme);
        applyMode(savedMode);

        // Mark theme as loaded (enables CSS transitions)
        document.documentElement.setAttribute('data-theme-loaded', 'true');
    }

    /**
     * Apply theme to document
     * @param {string} theme - Theme name (minimal, dense, friendly, glass)
     */
    function applyTheme(theme) {
        const validTheme = validateTheme(theme);
        document.documentElement.setAttribute('data-theme', validTheme);
        safeSetStorage(STORAGE_KEYS.THEME, validTheme);

        // Update select element if it exists
        const themeSelect = document.getElementById('theme-select');
        if (themeSelect) {
            themeSelect.value = validTheme;
        }

        // Emit custom event for external listeners
        document.dispatchEvent(new CustomEvent('sasquatch:themeChanged', {
            detail: { theme: validTheme }
        }));
    }

    /**
     * Apply mode (light/dark) to document
     * @param {string} mode - Mode name (light, dark)
     */
    function applyMode(mode) {
        const validMode = validateMode(mode);
        document.documentElement.setAttribute('data-mode', validMode);
        safeSetStorage(STORAGE_KEYS.MODE, validMode);

        // Update toggle button aria-label
        const modeToggle = document.getElementById('mode-toggle');
        if (modeToggle) {
            modeToggle.setAttribute('aria-label',
                validMode === 'light' ? 'Switch to dark mode' : 'Switch to light mode'
            );
        }

        // Emit custom event for external listeners
        document.dispatchEvent(new CustomEvent('sasquatch:modeChanged', {
            detail: { mode: validMode }
        }));
    }

    /**
     * Toggle between light and dark mode
     */
    function toggleMode() {
        const currentMode = document.documentElement.getAttribute('data-mode') || DEFAULTS.MODE;
        const newMode = currentMode === 'light' ? 'dark' : 'light';
        applyMode(newMode);
    }

    /**
     * Set up event listeners once DOM is ready
     */
    function setupEventListeners() {
        // Theme selector
        const themeSelect = document.getElementById('theme-select');
        if (themeSelect) {
            // Set initial value from current theme
            const currentTheme = document.documentElement.getAttribute('data-theme') || DEFAULTS.THEME;
            themeSelect.value = currentTheme;

            themeSelect.addEventListener('change', function (e) {
                applyTheme(e.target.value);
            });
        }

        // Mode toggle button
        const modeToggle = document.getElementById('mode-toggle');
        if (modeToggle) {
            modeToggle.addEventListener('click', function () {
                toggleMode();
            });
        }

        // Keyboard shortcut: Ctrl/Cmd + Shift + D for dark mode toggle
        document.addEventListener('keydown', function (e) {
            if ((e.ctrlKey || e.metaKey) && e.shiftKey && e.key === 'D') {
                e.preventDefault();
                toggleMode();
            }
        });
    }

    /**
     * Check for system dark mode preference
     * Only used if no localStorage preference exists
     */
    function checkSystemPreference() {
        if (!localStorage.getItem(STORAGE_KEYS.MODE)) {
            const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
            if (prefersDark) {
                applyMode('dark');
            }
        }
    }

    // Initialize theme immediately (before DOM ready) to prevent flash
    initTheme();

    // Set up event listeners when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function () {
            setupEventListeners();
            checkSystemPreference();
        });
    } else {
        setupEventListeners();
        checkSystemPreference();
    }

    // Listen for system preference changes
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', function (e) {
        // Only auto-switch if user hasn't set a preference
        if (!localStorage.getItem(STORAGE_KEYS.MODE)) {
            applyMode(e.matches ? 'dark' : 'light');
        }
    });

    // Expose API for programmatic theme control
    window.SasquatchTheme = {
        setTheme: applyTheme,
        setMode: applyMode,
        toggleMode: toggleMode,
        getTheme: function () {
            return document.documentElement.getAttribute('data-theme') || DEFAULTS.THEME;
        },
        getMode: function () {
            return document.documentElement.getAttribute('data-mode') || DEFAULTS.MODE;
        }
    };
})();
