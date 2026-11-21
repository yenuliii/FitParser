window.downloadFile = function (fileName, contentType, fileContents) {
    const blob = new Blob(
        [fileContents],
        { type: contentType }
    );

    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");

    link.href = url;
    link.download = fileName;
    link.style.display = "none";

    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);

    URL.revokeObjectURL(url);
};