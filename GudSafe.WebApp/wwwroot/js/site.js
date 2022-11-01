﻿// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

window.galleryData = window.galleryData || {
    currentPage: 1,
    connection: new signalR.HubConnectionBuilder().withUrl("/uploadHub").build()
};

$(function () {
    window.history.pushState({newUrl: window.location.pathname}, '', window.location.pathname);

    initLinks();

    loadPage(window.location.pathname);

    refreshDashboardActiveClass(window.location.pathname);
});

window.onpopstate = function (e) {
    if (e.state) {
        loadPage(e.state.newUrl);
        refreshDashboardActiveClass(e.state.newUrl);
    }
}

function initLinks() {
    $("a.btn").each(function () {
        $(this).click(navCallback);
    });

    $("a.nav-link:not(.dropdown-toggle)").each(function () {
        $(this).click(navCallback);
    });

    $("a.dropdown-item").each(function () {
        $(this).click(dropdownCallback);
    });
}

function dropdownCallback(e) {
    e.preventDefault();
    e.stopPropagation();

    let url = $(this).attr("href");

    clickHandler(url);
    
    $(this).closest(".dropdown").find(".dropdown-toggle").dropdown("toggle");
}

function navCallback(e) {
    e.preventDefault();
    e.stopPropagation();

    let url = $(this).attr("href");

    clickHandler(url);
}

function clickHandler(url) {
    if (url === window.location.pathname)
        return;

    if (window.galleryData.connection && !url.includes("gallery")) {
        window.galleryData.connection.stop();
    }

    loadPage(url);

    window.history.pushState({newUrl: url}, '', url);

    refreshDashboardActiveClass(url);
}

function loadPage(url) {
    $.ajax({
        url: url,
        type: "POST",
        success: function (data) {
            $("#page-content").html(data);
        }
    });
}

function refreshDashboardActiveClass(url) {
    console.log(url);

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