// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

window.galleryData = window.galleryData || {
    currentPage: 1,
    connection: new signalR.HubConnectionBuilder().withUrl("/uploadHub").build()
};

$(function () {
    // window.history.pushState({newUrl: window.location.pathname.concat(window.location.search)}, '', window.location.pathname.concat(window.location.search));

    initLinks();

    loadPage(window.location.pathname.concat(window.location.search), onSuccess);

    // refreshDashboardActiveClass(window.location.pathname);
});

window.onpopstate = async function (e) {
    if (e.state) {
        await loadPage(e.state.newUrl);
        refreshDashboardActiveClass(window.location.pathname);
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
    if (url === window.location.pathname.concat(window.location.search)) {
        return;
    }

    loadPage(url, onSuccess);
}

function onSuccess(url) {
    window.history.pushState({newUrl: url}, '', url);
    
    refreshDashboardActiveClass(window.location.pathname);
}

async function loadPage(url, successCallback) {
    if (window.galleryData.connection && !url.includes("gallery")) {
        await window.galleryData.connection.stop();
    }

    $.ajax({
        xhr: function () {
            let xhr = new XMLHttpRequest();

            xhr.onloadend = function () {
                if (successCallback) {
                    successCallback(xhr.responseURL);
                }
            };

            return xhr;
        },
        url: url,
        type: "GET",
        success: function (data, status, xhr) {
            let content = xhr.getResponseHeader("Content-Disposition");

            if (content && content.includes("attachment")) {
                window.location = url;
                return;
            }

            $("#page-content").html(data);
        },
        error: function (xhr, status, error) {
            if (xhr.status === 403) {
                window.location.href = xhr.responseJSON.redirectUrl;
            }
        }
    });
}

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