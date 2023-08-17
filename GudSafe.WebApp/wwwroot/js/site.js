// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

let isDark = (localStorage.getItem('isDark') || "false") === "true";

window.galleryData = window.galleryData || {
    currentPage: 1,
    connection: new signalR.HubConnectionBuilder().withUrl("/uploadHub").build()
};

$(function () {
    let mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');

    mediaQuery.onchange = function (e) {
        toggleDarkMode(e.matches);
    };
    
    refreshDashboardActiveClass(window.location.pathname);
});

function refreshDashboardActiveClass(url) {
    $(".navbar-nav a").removeClass("active");

    let navbarNav = $(".navbar-nav a[href='" + url + "']");

    navbarNav.addClass("active");

    navbarNav.closest(".dropdown").find(".dropdown-toggle").addClass("active");

    $("#dashboard-nav a").removeClass("active");
    $("#dashboard-nav a[href='" + url + "']").addClass("active");
}

function setWindowTitle(title) {
    document.title = title;
    $("#window-title").html(title);
}

function toggleDarkMode(newVal) {
    if (!newVal) {
        newVal = !isDark;
    }

    if (newVal) {
        document.documentElement.setAttribute('data-bs-theme', 'dark');
    } else {
        document.documentElement.setAttribute('data-bs-theme', 'light');
    }

    updateIcons(newVal);

    localStorage.setItem('isDark', newVal);
    isDark = newVal;
}

function updateIcons(dark) {
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