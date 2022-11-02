// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

let isDark = (localStorage.getItem('isDark') || "false") === "true";

$(function () {
    let mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');

    mediaQuery.onchange = function (e) {
        toggleDarkMode(e.matches);
    };
})

function toggleDarkMode(newVal) {
    if (!newVal) {
        newVal = !isDark;
    }

    if (newVal) {
        $("html").addClass('alt-colors');
    } else {
        $("html").removeClass('alt-colors');
    }
    
    updateIcons(newVal);

    localStorage.setItem('isDark', newVal);
    isDark = newVal;
}

function updateIcons(dark)
{
    let darkModeIcon = $("#dark-icon");
    let darkModeToggle = $("#dark-toggle");

    if (dark) {
        darkModeIcon.removeClass('bi-sun');
        darkModeToggle.removeClass('bi-toggle-on');

        darkModeIcon.addClass('bi-moon');
        darkModeToggle.addClass('bi-toggle-off');
    } else {
        darkModeIcon.removeClass('bi-moon');
        darkModeToggle.removeClass('bi-toggle-off');

        darkModeIcon.addClass('bi-sun');
        darkModeToggle.addClass('bi-toggle-on');
    }
}