const chunkSize = 25 * 1000 * 1000;

async function uploadFile() {
    let file = $("#formFile")[0].files[0];

    let uploadId = "";
    let curChunk = 0;
    let chunkCount = Math.ceil(file.size / chunkSize);

    console.log("Uploading file " + file.name + " with " + chunkCount + " chunks");

    $("#progress-bar").show();

    let progress = $("#progress");

    do {
        let formData = new FormData();

        formData.append("file", file.slice(curChunk * chunkSize, (curChunk + 1) * chunkSize), file.name);
        formData.append("chunk", curChunk.toString());
        formData.append("chunkCount", chunkCount.toString());
        formData.append("uploadId", uploadId);

        await $.ajax({
            url: "/Dashboard/UploadFile",
            type: "POST",
            data: formData,
            processData: false,
            contentType: false,
            xhr: function () {
                let xhr = new window.XMLHttpRequest();
                xhr.upload.addEventListener("progress", function (evt) {
                    if (evt.lengthComputable) {
                        let percentComplete = evt.loaded / evt.total;
                        console.log("Upload progress: " + percentComplete);

                        let offset = (100 / chunkCount) * curChunk;
                        let value = (percentComplete * 100) / chunkCount;

                        progress.css("width", value + offset + "%");
                        progress.html(Math.round(value + offset) + "%");
                    }
                }, false);
                return xhr;
            },
            success: function (result) {
                console.log(result);
                if (result.success) {
                    uploadId = result.uploadId;
                }
            },
        });

        curChunk++;
    } while (curChunk < chunkCount);
}