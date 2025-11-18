// jQuery Validate Türkçeleþtirme
// Kaynak: https://github.com/jquery-validation/jquery-validation/tree/master/src/localization
(function ($) {
    if (!$.validator || !$.validator.messages) return;
    $.extend($.validator.messages, {
        required: "Bu alanýn doldurulmasý zorunludur.",
        remote: "Lütfen bu alaný düzeltin.",
        email: "Lütfen geçerli bir e-posta adresi girin.",
        url: "Lütfen geçerli bir URL girin.",
        date: "Lütfen geçerli bir tarih girin.",
        dateISO: "Lütfen geçerli bir ISO tarih girin.",
        number: "Lütfen geçerli bir sayý girin.",
        digits: "Lütfen sadece rakam girin.",
        creditcard: "Lütfen geçerli bir kredi kartý girin.",
        equalTo: "Lütfen ayný deðeri tekrar girin.",
        maxlength: $.validator.format("Lütfen en fazla {0} karakter girin."),
        minlength: $.validator.format("Lütfen en az {0} karakter girin."),
        rangelength: $.validator.format("Lütfen {0} ile {1} arasýnda karakter girin."),
        range: $.validator.format("Lütfen {0} ile {1} arasýnda bir deðer girin."),
        max: $.validator.format("Lütfen {0} deðerine eþit ya da daha küçük bir deðer girin."),
        min: $.validator.format("Lütfen {0} deðerine eþit ya da daha büyük bir deðer girin."),
        step: $.validator.format("Lütfen {0} sayýsýnýn katý bir deðer girin.")
    });
}(jQuery));
