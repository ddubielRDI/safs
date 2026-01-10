// =============================================================================
// SAFS Gulp Build Configuration
// =============================================================================

const { src, dest, series, parallel, watch } = require('gulp');
const sass = require('gulp-sass')(require('sass'));
const cleanCSS = require('gulp-clean-css');
const uglify = require('gulp-uglify');
const rename = require('gulp-rename');
const sourcemaps = require('gulp-sourcemaps');
const gulpIf = require('gulp-if');
const del = require('del');

// -----------------------------------------------------------------------------
// Configuration
// -----------------------------------------------------------------------------
// Supports both 'production' and 'Release' (from MSBuild)
const isProduction = process.env.NODE_ENV === 'production' || process.env.NODE_ENV === 'Release';

const paths = {
    scss: {
        src: 'Assets/scss/**/*.scss',
        entry: 'Assets/scss/site.scss',
        dest: 'wwwroot/css/'
    },
    js: {
        src: 'Assets/js/**/*.js',
        dest: 'wwwroot/js/'
    },
    libs: {
        bootstrap: {
            css: 'node_modules/bootstrap/dist/css/bootstrap.min.css',
            js: 'node_modules/bootstrap/dist/js/bootstrap.bundle.min.js'
        },
        jquery: 'node_modules/jquery/dist/jquery.min.js',
        jqueryValidation: [
            'node_modules/jquery-validation/dist/jquery.validate.min.js',
            'node_modules/jquery-validation/dist/additional-methods.min.js'
        ],
        jqueryValidationUnobtrusive: 'node_modules/jquery-validation-unobtrusive/dist/jquery.validate.unobtrusive.min.js'
    },
    images: {
        src: 'Assets/images/**/*',
        dest: 'wwwroot/images/'
    },
    fonts: {
        src: 'Assets/fonts/**/*',
        dest: 'wwwroot/fonts/'
    }
};

// -----------------------------------------------------------------------------
// Clean Task
// -----------------------------------------------------------------------------
function clean() {
    return del([
        'wwwroot/css/**',
        'wwwroot/js/**',
        'wwwroot/lib/**',
        'wwwroot/images/**',
        'wwwroot/fonts/**',
        '!wwwroot/.gitkeep'
    ]);
}

// -----------------------------------------------------------------------------
// SCSS Task
// -----------------------------------------------------------------------------
function scss() {
    return src(paths.scss.entry)
        .pipe(gulpIf(!isProduction, sourcemaps.init()))
        .pipe(sass().on('error', sass.logError))
        .pipe(gulpIf(isProduction, cleanCSS({ level: 2 })))
        .pipe(gulpIf(!isProduction, sourcemaps.write('.')))
        .pipe(dest(paths.scss.dest));
}

// -----------------------------------------------------------------------------
// JavaScript Task
// -----------------------------------------------------------------------------
function js() {
    return src(paths.js.src)
        .pipe(gulpIf(!isProduction, sourcemaps.init()))
        .pipe(gulpIf(isProduction, uglify()))
        .pipe(gulpIf(!isProduction, sourcemaps.write('.')))
        .pipe(dest(paths.js.dest));
}

// -----------------------------------------------------------------------------
// Library Tasks
// -----------------------------------------------------------------------------
function libBootstrapCSS() {
    return src(paths.libs.bootstrap.css)
        .pipe(dest('wwwroot/lib/bootstrap/css/'));
}

function libBootstrapJS() {
    return src(paths.libs.bootstrap.js)
        .pipe(dest('wwwroot/lib/bootstrap/js/'));
}

function libJquery() {
    return src(paths.libs.jquery)
        .pipe(dest('wwwroot/lib/jquery/'));
}

function libJqueryValidation() {
    return src(paths.libs.jqueryValidation)
        .pipe(dest('wwwroot/lib/jquery-validation/'));
}

function libJqueryValidationUnobtrusive() {
    return src(paths.libs.jqueryValidationUnobtrusive)
        .pipe(dest('wwwroot/lib/jquery-validation-unobtrusive/'));
}

const libs = parallel(
    libBootstrapCSS,
    libBootstrapJS,
    libJquery,
    libJqueryValidation,
    libJqueryValidationUnobtrusive
);

// -----------------------------------------------------------------------------
// Images Task
// -----------------------------------------------------------------------------
function images() {
    return src(paths.images.src, { allowEmpty: true })
        .pipe(dest(paths.images.dest));
}

// -----------------------------------------------------------------------------
// Fonts Task
// -----------------------------------------------------------------------------
function fonts() {
    return src(paths.fonts.src, { allowEmpty: true })
        .pipe(dest(paths.fonts.dest));
}

// -----------------------------------------------------------------------------
// Watch Task
// -----------------------------------------------------------------------------
function watchFiles() {
    watch(paths.scss.src, scss);
    watch(paths.js.src, js);
    watch(paths.images.src, images);
    watch(paths.fonts.src, fonts);
}

// -----------------------------------------------------------------------------
// Export Tasks
// -----------------------------------------------------------------------------
exports.clean = clean;
exports.scss = scss;
exports.js = js;
exports.libs = libs;
exports.images = images;
exports.fonts = fonts;
exports.watch = series(parallel(scss, js, libs, images, fonts), watchFiles);
exports.build = series(clean, parallel(scss, js, libs, images, fonts));
exports.default = exports.build;
