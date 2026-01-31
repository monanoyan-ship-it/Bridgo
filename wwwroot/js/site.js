// Bridgo B2B Platform - Global JavaScript

// =====================================================
// LOCALIZATION SYSTEM
// JavaScript tarafinda T() fonksiyonu ile ceviri
// =====================================================

var Bridgo = Bridgo || {};

Bridgo.Localization = (function () {
    var _resources = {};
    var _culture = 'tr-TR';
    var _isLoaded = false;
    var _loadPromise = null;

    // Ceviri kaynaklarini yukle
    function load() {
        if (_loadPromise) return _loadPromise;

        _loadPromise = $.ajax({
            url: '/api/localization/resources',
            method: 'GET',
            dataType: 'json'
        }).then(function (data) {
            _resources = data.resources || {};
            _culture = data.culture || 'tr-TR';
            _isLoaded = true;
            return _resources;
        }).catch(function (error) {
            console.error('Localization resources could not be loaded:', error);
            _isLoaded = true; // Fallback moduna gec
            return {};
        });

        return _loadPromise;
    }

    // Ceviri al - senkron (onceden yuklenmis olmali)
    function get(key, defaultValue) {
        if (!key) return defaultValue || '';

        var value = _resources[key];
        if (value !== undefined) return value;

        // Fallback: key'in kendisini dondur
        return defaultValue || key;
    }

    // Format parametreli ceviri
    // Ornek: T('Common.Welcome', 'Ahmet') -> 'Hosgeldiniz, Ahmet'
    function format(key, ...args) {
        var template = get(key);

        if (args.length === 0) return template;

        // {0}, {1}, ... seklindeki placeholder'lari degistir
        return template.replace(/\{(\d+)\}/g, function (match, index) {
            return args[index] !== undefined ? args[index] : match;
        });
    }

    // Mevcut kultur bilgisi
    function getCulture() {
        return _culture;
    }

    // Yuklendi mi?
    function isLoaded() {
        return _isLoaded;
    }

    return {
        load: load,
        get: get,
        format: format,
        getCulture: getCulture,
        isLoaded: isLoaded
    };
})();

// Global T() fonksiyonu - kolay kullanim icin
function T(key, ...args) {
    if (args.length > 0) {
        return Bridgo.Localization.format(key, ...args);
    }
    return Bridgo.Localization.get(key);
}

// Sayfa yuklendiginde localization'i yukle
$(document).ready(function () {
    Bridgo.Localization.load();
});
