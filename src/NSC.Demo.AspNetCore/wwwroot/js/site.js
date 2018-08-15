// Intl.DateTimeFormat().resolvedOptions().timeZone
// Intl.NumberFormat().resolvedOptions()
NSC.Tools = NSC.Tools || {};

//var car = { color: 'white', wheels: 4 };
//console.log(car);
//localStorage.setItem('car', JSON.stringify(car));
//console.log(JSON.parse(localStorage.getItem('car')));
//localStorage.removeItem('car');
//var taste = localStorage.getItem('car'); => null

// Prevent form validation on inputs if enter is pressed
$(document).ready(function () { $('input').not("[type='button']").keydown(function (event) { if (event.keyCode === 13) { event.preventDefault(); return false; } }); });


NSC.loadingFunctions = NSC.loadingFunctions || {}
// Only takes one call into account
NSC.loadingFunctions.ajaxStart = NSC.loadingFunctions.ajaxStart || []; NSC.loadingFunctions.ajaxStart.push(function () { $('body').addClass('ajax-loading'); });
NSC.loadingFunctions.ajaxEnd = NSC.loadingFunctions.ajaxEnd || []; NSC.loadingFunctions.ajaxEnd.push(function () { $('body').removeClass('ajax-loading'); });
NSC.loadingFunctions.callAjaxStart = function () {
    if (NSC.loadingFunctions.ajaxStart)
        for (var i = 0; i < NSC.loadingFunctions.ajaxStart.length; i++)
            NSC.loadingFunctions.ajaxStart[i]();
}
NSC.loadingFunctions.callAjaxEnd = function () {
    if (NSC.loadingFunctions.ajaxEnd)
        for (var i = 0; i < NSC.loadingFunctions.ajaxEnd.length; i++)
            NSC.loadingFunctions.ajaxEnd[i]();
}
$(document).on({
    ajaxStart: function () {
        NSC.loadingFunctions.callAjaxStart();
    },
    ajaxStop: function () {
        NSC.loadingFunctions.callAjaxEnd();
    },
    ajaxComplete: function () {
        NSC.loadingFunctions.callAjaxEnd();
    },
    ajaxError: function () {
        NSC.loadingFunctions.callAjaxEnd();
    }
});


NSC.goToUrl = function (url) {
    window.location = url;
    $('.loading-page').addClass('exit');
    window.scrollTo(0, 0);
}

NSC.refresh = function () { window.location.reload(true); }
NSC.serializeForm = function (form) {
    var data = form.serializeArray().reduce(function (obj, item) {
        if (!obj[item.name]) obj[item.name] = item.value;
        else {
            // Check if ASP checkbox, in that case keep only the "true" value
            if (obj[item.name] === "false" && item.value === "true") obj[item.name] = item.value;
            else if (obj[item.name] === "true" && item.value === "false");
            // else Concatenate values in an array
            else {
                if (!Array.isArray(obj[item.name]))
                    obj[item.name] = [obj[item.name]];
                obj[item.name].push(item.value);
            }
        }
        return obj;
    }, {});
    return data;
}

NSC.notif = function (text, type, title, delay) {
    // type:  'notice', 'info', 'success', 'error'.
    if (delay) {
        new PNotify({ text: text, type: type, title: title, delay: delay });
    }
    else if (title) {
        new PNotify({ text: text, type: type, title: title });
    } else if (type) {
        new PNotify({ text: text, type: type });
    } else {
        new PNotify({ text: text });
    }
}

NSC.confirmNotify = function (title, text, type, confirm, cancel) {
    var notice = new PNotify({
        title: title, text: text, type: type, hide: false,
        confirm: {
            confirm: true
        },
        buttons: {
            closer: false,
            sticker: false
        },
        history: {
            history: false
        },
        addclass: 'stack-modal',
        stack: { 'dir1': 'down', 'dir2': 'right', 'modal': true }
    });

    notice.get().on('click', function (e) { e.stopPropagation(); })
    if (confirm) notice.get().on('pnotify.confirm', confirm);
    if (cancel) notice.get().on('pnotify.cancel', cancel);
}


NSC.randomId = function () {
    return 'r' + Math.random().toString(36).replace(/[^a-z0-9]+/g, '').substr(2, 10);
}


NSC.sendForm = function (eltHtml, handlers) {
    var elt = $(eltHtml);
    var form = elt.closest('form');
    var url = form.attr('data-url');
    url = elt.attr('data-url') || url;
    var method = form.attr('data-method') || form.attr('method');
    if (elt.attr('data-method')) method = elt.attr('data-method');
    var data = NSC.serializeForm(form);
    NSC.genericAjax(method, url, data, handlers);
}

NSC.genericAjaxGet = function (url, data, handlers, opts) {
    NSC.genericAjax('get', url, data, handlers, opts);
}
NSC.genericAjaxPost = function (url, data, handlers, opts) {
    NSC.genericAjax('post', url, data, handlers, opts);
}
NSC.genericAjaxPatch = function (url, data, handlers, opts) {
    NSC.genericAjax('patch', url, data, handlers, opts);
}
NSC.genericAjaxDelete = function (url, data, handlers, opts) {
    NSC.genericAjax('delete', url, data, handlers, opts);
}

// Options: generic request options, or md_noSplit to cancel auto properties split
// Handlers: { success:function(result){}, error:function(response){}}  // result being response.result, response is the response object including error codes/messages etc and potential result.
NSC.genericAjax = function (method, url
    , data /* data object, not stringified */
    , handlers /* .success or .error */
    , opts /* to override some options */) {

    // Automatic split of {property1.subProperty1:XXX} to {property1: {subProperty1:XXX}}
    if (!opts || !opts.md_noSplit) {
        // TODO: multi-level
        var keysToSplit = [];
        for (var key in data) {
            if (key.includes('.'))
                keysToSplit.push({ fullKey: key, mainKey: key.split('.')[0], subKey: key.split('.')[1], value: data[key] });
        }
        for (var i = 0; i < keysToSplit.length; i++) {
            var key2 = keysToSplit[i];
            delete data[key2.fullKey];
            if (!data[key2.mainKey]) data[key2.mainKey] = {};
            data[key2.mainKey][key2.subKey] = key2.value;
        }
    }

    var req = {
        cache: false,
        type: method,
        method: method,
        url: url,
        contentType: 'application/json; charset=utf-8',
        data: data === null ? null : JSON.stringify(data),
        dataType: 'json',
        //        traditional: true,
        success: function (resp) {
            NSC.handleGenericSuccess(resp, handlers);
        },
        error: function (xhr, ajaxOptions, thrownError) {
            var resp = JSON.parse(xhr.responseText);
            NSC.handleGenericErrors(resp, handlers);
        },
        complete: function (xhr, textStatus) {
        }
    };
//    if (token) req.headers = { "RequestVerificationToken": token };
    if (opts) {
        for (var o in opts) if (!o.startsWith('md_')) req[o] = opts[o];
    }
    $.ajax(req);
}

NSC.loadingPage = function () {
    $('.loading-page').addClass('exit');
    window.scrollTo(0, 0);
}
NSC.handleGenericSuccess = function (resp, handlers) {
    if (resp.success) {
        if (resp.result && resp.result.redirectLink) {
            window.location = resp.result.redirectLink;
            NSC.loadingPage();
        }
        if (handlers && handlers.success)
            handlers.success(resp.result);
    }
    else {
        NSC.handleGenericErrors(resp);
        if (handlers && handlers.error)
            handlers.error(resp);
    }
}

NSC.handleGenericErrors = function (resp, handlers) {
    var errorCodes = [];
    if (resp.errorCodes) errorCodes = resp.errorCodes;
    else if (resp.errorCode) errorCodes = [resp.errorCode];

    var errorMsgs = [];
    if (resp.errors) errorMsgs = resp.errors;
    else if (resp.error) errorMsgs = [resp.error];

    if (!PNotify) {
        if (console.error) console.error('PNotify missing...');
        else console.log('PNotify missing...');
    }

    if (errorMsgs && errorMsgs.length > 0) {
        for (var i = 0; i < errorMsgs.length; i++)
            new PNotify({ text: errorMsgs[i], type: 'error' });
    } else if (errorCodes && errorCodes.length > 0) {
        for (var j = 0; j < errorCodes.length; j++) {
            var msg = NSC.getErrorMessage(errorCodes[j]);
            new PNotify({ text: msg, type: 'error' });
        }
    }

    if (handlers && handlers.error)
        handlers.error(resp);
}


String.prototype.format = function (a0, a1, a2, a3, a4, a5, a6, a7, a8, a9) {
    var str = this;
    if (a0 !== undefined) {
        str = str.replace('{0}', a0);
        if (a1 !== undefined) {
            str = str.replace('{1}', a1);
            if (a2 !== undefined) {
                str = str.replace('{2}', a2);
                if (a3 !== undefined) {
                    str = str.replace('{3}', a3);
                    if (a4 !== undefined) {
                        str = str.replace('{4}', a4);
                        if (a5 !== undefined) {
                            str = str.replace('{5}', a5);
                            if (a6 !== undefined) {
                                str = str.replace('{6}', a6);
                                if (a7 !== undefined) {
                                    str = str.replace('{7}', a7);
                                    if (a8 !== undefined) {
                                        str = str.replace('{8}', a8);
                                        if (a9 !== undefined) {
                                            str = str.replace('{9}', a9);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    return str;
}


/* Dates */
Date.prototype.addSeconds = function (seconds) {
    this.setSeconds(this.getSeconds() + seconds);
    return this;
};

Date.prototype.addMinutes = function (minutes) {
    this.setMinutes(this.getMinutes() + minutes);
    return this;
};

Date.prototype.addHours = function (hours) {
    this.setHours(this.getHours() + hours);
    return this;
};

Date.prototype.addDays = function (days) {
    this.setDate(this.getDate() + days);
    return this;
};

Date.prototype.addWeeks = function (weeks) {
    this.addDays(weeks * 7);
    return this;
};

Date.prototype.addMonths = function (months) {
    var dt = this.getDate();

    this.setMonth(this.getMonth() + months);
    var currDt = this.getDate();

    if (dt !== currDt) {
        this.addDays(-currDt);
    }

    return this;
};

Date.prototype.addYears = function (years) {
    var dt = this.getDate();

    this.setFullYear(this.getFullYear() + years);

    var currDt = this.getDate();

    if (dt !== currDt) {
        this.addDays(-currDt);
    }

    return this;
};

/* end Dates */


/// Simulate a checkbox
/// see .btn-chkbox[type="hidden"]:not(.checked) + label.btn-chkbox .checked
NSC.switchCheckBox = function (elt2) {
    var elt = $(elt2);
    if (elt.length === 0) throw "Cannot find checkbox " + elt2;
    var newVal = !elt.hasClass('checked');
    elt.val(newVal);
    elt.toggleClass('checked');
}
