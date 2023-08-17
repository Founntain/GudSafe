$(async function () {
    try {
        await window.galleryData.connection.start();
    } catch (err) {
        console.log(err);
    }
});

window.galleryData.connection.on("RefreshFiles", function () {
    loadGalleryPage(1);
});

function loadGalleryPage(pageNumber) {
    $.ajax({
        url: "/Dashboard/Gallery",
        type: 'GET',
        data: {pageNumber: pageNumber},
        success: function (data) {
            window.galleryData.currentPage = pageNumber;
            
            console.log(data);
            $('#ajax-div').html(data);
        }
    });
}

function deleteFile(id) {
    $.ajax({
        url: "/Dashboard/DeleteFile",
        type: "POST",
        contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
        data: {id: id},
        success: function (result) {
            if (result.success) {
                loadGalleryPage(window.galleryData.currentPage);
            }
        }
    });
}