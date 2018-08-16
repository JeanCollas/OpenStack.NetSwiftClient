; (function ($) {
    $.fn.html5Uploader = function (options) {

        var crlf = '\r\n';
        var boundary = "iloveigloo";
        var dashes = "--";

        var settings = {
            "name": "File",
            "postUrl": "/Upload",
            "onClientAbort": null,
            "onClientError": null,
            "onClientLoad": null,
            "onClientLoadEnd": null,
            "onClientLoadStart": null,
            "onClientProgress": null,
            "onServerAbort": null,
            "onServerError": null,
            "onServerLoad": null,
            "onServerLoadStart": null,
            "onServerProgress": null,
            "onServerReadyStateChange": null,
            "onStart": null, // fx(file, inputElt)
            "onProgress": null, // fx(arg, file, inputElt, progress)
            "onEnd": null, // fx(arg, file, inputElt, success)
            "onSuccess": null
        };

        if (options) {
            $.extend(settings, options);
        }

        return this.each(function (options) {
            var $this = $(this);

            if ($this.is("[type='file']")) {
                $this
                    .bind("change", function () {
                        var files = this.files;
                        for (var i = 0; i < files.length; i++) {
                            fileHandler(files[i], $this);
                        }
                    });
            } else {
                $this
                    .bind("dragenter dragover", function () {
                        $(this).addClass("hover");
                        return false;
                    })
                    .bind("dragleave", function () {
                        $(this).removeClass("hover");
                        return false;
                    })
                    .bind("drop", function (e) {
                        $(this).removeClass("hover");

                        var files = e.originalEvent.dataTransfer.files;
                        for (var i = 0; i < files.length; i++) {
                            fileHandler(files[i], $this);
                        }
                        return false;
                    });
            }
        });

        function fileHandler(file, elt) {

            if (settings.onStart) settings.onStart(file, elt);

            var fileReader = new FileReader();
            fileReader.onabort = function (e) {
                if (settings.onClientAbort) {
                    settings.onClientAbort(e, file);
                }
                if (settings.onEnd) {
                    settings.onEnd(e, file, elt, false);
                }
            };
            fileReader.onerror = function (e) {
                if (settings.onClientError) {
                    settings.onClientError(e, file);
                }
                if (settings.onEnd) {
                    settings.onEnd(e, file, elt, false);
                }
            };
            fileReader.onload = function (e) {
                if (settings.onClientLoad) {
                    settings.onClientLoad(e, file);
                }
            };
            fileReader.onloadend = function (e) {
                if (settings.onClientLoadEnd) {
                    settings.onClientLoadEnd(e, file);
                }
            };
            fileReader.onloadstart = function (e) {
                if (settings.onClientLoadStart) {
                    settings.onClientLoadStart(e, file);
                }
            };
            fileReader.onprogress = function (e) {
                if (settings.onClientProgress) {
                    settings.onClientProgress(e, file);
                }
            };
            fileReader.readAsDataURL(file);

            var xmlHttpRequest = new XMLHttpRequest();
            xmlHttpRequest.upload.onabort = function (e) {
                if (settings.onServerAbort) {
                    settings.onServerAbort(e, file);
                }
                if (settings.onEnd) {
                    settings.onEnd(e, file, elt, false);
                }
            };
            xmlHttpRequest.upload.onerror = function (e) {
                if (settings.onServerError) {
                    settings.onServerError(e, file);
                }
                if (settings.onEnd) {
                    settings.onEnd(e, file, elt, false);
                }
            };
            xmlHttpRequest.upload.onload = function (e) {
                if (settings.onServerLoad) {
                    settings.onServerLoad(e, file);
                }
            };
            xmlHttpRequest.upload.onloadstart = function (e) {
                if (settings.onServerLoadStart) {
                    settings.onServerLoadStart(e, file);
                }
            };
            xmlHttpRequest.upload.onprogress = function (e) {
                if (settings.onServerProgress) {
                    settings.onServerProgress(e, file);
                }
                if (settings.onProgress && e.lengthComputable) {
                    var percentComplete = e.loaded / e.total * 100;
                    settings.onProgress(e, file, elt, percentComplete);
                }
            };
            xmlHttpRequest.onreadystatechange = function (e) {
                if (settings.onServerReadyStateChange) {
                    settings.onServerReadyStateChange(e, file, xmlHttpRequest.readyState);
                }
                if (settings.onSuccess && xmlHttpRequest.readyState === 4 && xmlHttpRequest.status === 200) {
                    settings.onSuccess(e, file, elt, xmlHttpRequest.responseText);
                }
                if (settings.onEnd && xmlHttpRequest.readyState === 4) {
                    settings.onEnd(e, file, elt, xmlHttpRequest.status === 200);
                }
            };
            xmlHttpRequest.open("POST", settings.postUrl, true);

            if (window.FormData) { // Chrome

                var formData = new FormData();
                formData.append(settings.name, file);

                xmlHttpRequest.send(formData);
            }
            else if (file.getAsBinary) { // Firefox

                var data = dashes + boundary + crlf +
                    "Content-Disposition: form-data;" +
                    "name=\"" + settings.name + "\";" +
                    "filename=\"" + unescape(encodeURIComponent(file.name)) + "\"" + crlf +
                    "Content-Type: application/octet-stream" + crlf + crlf +
                    file.getAsBinary() + crlf +
                    dashes + boundary + dashes;

                xmlHttpRequest.setRequestHeader("Content-Type", "multipart/form-data;boundary=" + boundary);
                xmlHttpRequest.sendAsBinary(data);
            }
        }

    };
})(jQuery);