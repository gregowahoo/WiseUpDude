window.wiseUpDudeDropZone = {
    init: function (dropZoneElem, dotNetHelper) {
        if (!dropZoneElem) return;
        dropZoneElem.addEventListener('dragover', function (e) {
            e.preventDefault();
        });
        dropZoneElem.addEventListener('drop', function (e) {
            e.preventDefault();
            let url = e.dataTransfer.getData('text/uri-list') || e.dataTransfer.getData('text/plain');
            if (url) {
                dotNetHelper.invokeMethodAsync('HandleDroppedUrl', url);
            }
        });
    }
};
