// jQuery Validate Turkcelestirme
(function ($) {
    if (!$.validator || !$.validator.messages) return;
    $.extend($.validator.messages, {
        required: "Bu alan\u0131n doldurulmas\u0131 zorunludur.",
        remote: "L\u00fctfen bu alan\u0131 d\u00fczeltin.",
        email: "L\u00fctfen ge\u00e7erli bir e-posta adresi girin.",
        url: "L\u00fctfen ge\u00e7erli bir URL girin.",
        date: "L\u00fctfen ge\u00e7erli bir tarih girin.",
        dateISO: "L\u00fctfen ge\u00e7erli bir ISO tarih girin.",
        number: "L\u00fctfen ge\u00e7erli bir say\u0131 girin.",
        digits: "L\u00fctfen sadece rakam girin.",
        creditcard: "L\u00fctfen ge\u00e7erli bir kredi kart\u0131 girin.",
        equalTo: "L\u00fctfen ayn\u0131 de\u011feri tekrar girin.",
        maxlength: $.validator.format("L\u00fctfen en fazla {0} karakter girin."),
        minlength: $.validator.format("L\u00fctfen en az {0} karakter girin."),
        rangelength: $.validator.format("L\u00fctfen {0} ile {1} aras\u0131nda karakter girin."),
        range: $.validator.format("L\u00fctfen {0} ile {1} aras\u0131nda bir de\u011fer girin."),
        max: $.validator.format("L\u00fctfen {0} de\u011ferine e\u015fit ya da daha k\u00fc\u00e7\u00fck bir de\u011fer girin."),
        min: $.validator.format("L\u00fctfen {0} de\u011ferine e\u015fit ya da daha b\u00fcy\u00fck bir de\u011fer girin."),
        step: $.validator.format("L\u00fctfen {0} say\u0131s\u0131n\u0131n kat\u0131 bir de\u011fer girin.")
    });
}(jQuery));
