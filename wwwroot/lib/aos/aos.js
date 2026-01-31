/**
 * AOS - Animate On Scroll Library
 * Version: 2.3.4
 * https://michalsnik.github.io/aos/
 * MIT License
 */
(function (global, factory) {
  typeof exports === 'object' && typeof module !== 'undefined' ? module.exports = factory() :
  typeof define === 'function' && define.amd ? define(factory) :
  (global = global || self, global.AOS = factory());
}(this, (function () { 'use strict';

  var throttle = function throttle(func, wait, options) {
    var context, args, result;
    var timeout = null;
    var previous = 0;
    if (!options) options = {};
    var later = function later() {
      previous = options.leading === false ? 0 : Date.now();
      timeout = null;
      result = func.apply(context, args);
      if (!timeout) context = args = null;
    };
    return function () {
      var now = Date.now();
      if (!previous && options.leading === false) previous = now;
      var remaining = wait - (now - previous);
      context = this;
      args = arguments;
      if (remaining <= 0 || remaining > wait) {
        if (timeout) {
          clearTimeout(timeout);
          timeout = null;
        }
        previous = now;
        result = func.apply(context, args);
        if (!timeout) context = args = null;
      } else if (!timeout && options.trailing !== false) {
        timeout = setTimeout(later, remaining);
      }
      return result;
    };
  };

  var debounce = function debounce(func, wait, immediate) {
    var timeout;
    return function () {
      var context = this;
      var args = arguments;
      var later = function later() {
        timeout = null;
        if (!immediate) func.apply(context, args);
      };
      var callNow = immediate && !timeout;
      clearTimeout(timeout);
      timeout = setTimeout(later, wait);
      if (callNow) func.apply(context, args);
    };
  };

  var isNumeric = function isNumeric(value) {
    return !isNaN(parseFloat(value)) && isFinite(value);
  };

  var offset = function offset(el) {
    var _x = 0;
    var _y = 0;
    while (el && !isNaN(el.offsetLeft) && !isNaN(el.offsetTop)) {
      _x += el.offsetLeft - (el.tagName !== 'BODY' ? el.scrollLeft : 0);
      _y += el.offsetTop - (el.tagName !== 'BODY' ? el.scrollTop : 0);
      el = el.offsetParent;
    }
    return { top: _y, left: _x };
  };

  var getInlineOption = function getInlineOption(el, key, def) {
    var attr = el.getAttribute('data-aos-' + key);
    if (typeof attr !== 'undefined') {
      if (attr === 'true') return true;
      if (attr === 'false') return false;
    }
    return attr || def;
  };

  var prepare = function prepare(elements, options) {
    elements.forEach(function (el, i) {
      var mirror = getInlineOption(el, 'mirror', options.mirror);
      var once = getInlineOption(el, 'once', options.once);
      var id = getInlineOption(el, 'id');
      var customAnchor = getInlineOption(el, 'anchor');
      var anchorPlacement = getInlineOption(el, 'anchor-placement', options.anchorPlacement);
      var offsetVal = getInlineOption(el, 'offset', options.offset);
      var delay = getInlineOption(el, 'delay', options.delay);
      var duration = getInlineOption(el, 'duration', options.duration);
      var easing = getInlineOption(el, 'easing', options.easing);

      var anchor = customAnchor && document.querySelectorAll(customAnchor) ? document.querySelectorAll(customAnchor)[0] : el;
      var finalOffset = isNumeric(offsetVal) ? parseInt(offsetVal) : 0;

      var triggerPosition = offset(anchor).top - window.innerHeight;

      switch (anchorPlacement) {
        case 'top-bottom':
          break;
        case 'center-bottom':
          triggerPosition += anchor.offsetHeight / 2;
          break;
        case 'bottom-bottom':
          triggerPosition += anchor.offsetHeight;
          break;
        case 'top-center':
          triggerPosition += window.innerHeight / 2;
          break;
        case 'center-center':
          triggerPosition += window.innerHeight / 2 + anchor.offsetHeight / 2;
          break;
        case 'bottom-center':
          triggerPosition += window.innerHeight / 2 + anchor.offsetHeight;
          break;
        case 'top-top':
          triggerPosition += window.innerHeight;
          break;
        case 'center-top':
          triggerPosition += window.innerHeight + anchor.offsetHeight / 2;
          break;
        case 'bottom-top':
          triggerPosition += window.innerHeight + anchor.offsetHeight;
          break;
      }

      el.setAttribute('data-aos-trigger', triggerPosition + finalOffset);
      el.setAttribute('data-aos-mirror', mirror);
      el.setAttribute('data-aos-once', once);

      if (delay) el.style.transitionDelay = delay + 'ms';
      if (duration) el.style.transitionDuration = duration + 'ms';
      if (easing) el.style.transitionTimingFunction = easing;
    });
  };

  var handleScroll = function handleScroll(elements, options) {
    var scrollTop = window.pageYOffset;
    elements.forEach(function (el) {
      var trigger = parseInt(el.getAttribute('data-aos-trigger'));
      var mirror = el.getAttribute('data-aos-mirror') === 'true';
      var once = el.getAttribute('data-aos-once') === 'true';
      var isAnimated = el.classList.contains('aos-animate');

      if (scrollTop > trigger) {
        if (!isAnimated) {
          el.classList.add('aos-animate');
        }
      } else {
        if (isAnimated && !once) {
          el.classList.remove('aos-animate');
        }
      }
    });
  };

  var init = function init() {
    var elements = arguments.length > 0 && arguments[0] !== undefined ? arguments[0] : [];
    var options = arguments.length > 1 && arguments[1] !== undefined ? arguments[1] : {};

    prepare(elements, options);
    handleScroll(elements, options);
  };

  var defaultOptions = {
    offset: 120,
    delay: 0,
    duration: 400,
    easing: 'ease',
    once: false,
    mirror: false,
    anchorPlacement: 'top-bottom',
    startEvent: 'DOMContentLoaded',
    animatedClassName: 'aos-animate',
    initClassName: 'aos-init',
    useClassNames: false,
    disableMutationObserver: false,
    throttleDelay: 99,
    debounceDelay: 50
  };

  var aosElements = [];
  var initialized = false;

  var $elements = [];
  var $options = {};

  var refresh = function refresh() {
    var initialize = arguments.length > 0 && arguments[0] !== undefined ? arguments[0] : false;

    if (initialize) initialized = true;
    if (!initialized) return;

    $elements = [].slice.call(document.querySelectorAll('[data-aos]'));

    if ($options.disableMutationObserver !== true && typeof MutationObserver !== 'undefined') {
      // Do nothing with mutation observer for now
    }

    $elements.forEach(function (el) {
      el.classList.add('aos-init');
    });

    prepare($elements, $options);
    handleScroll($elements, $options);
  };

  var hardRefresh = function hardRefresh() {
    $elements.forEach(function (el) {
      el.classList.remove('aos-init');
      el.classList.remove('aos-animate');
    });
    refresh(true);
  };

  var disable = function disable() {
    $elements.forEach(function (el) {
      el.removeAttribute('data-aos');
      el.removeAttribute('data-aos-trigger');
      el.removeAttribute('data-aos-mirror');
      el.removeAttribute('data-aos-once');
      el.classList.remove('aos-init');
      el.classList.remove('aos-animate');
    });
  };

  var aosInit = function aosInit(settings) {
    $options = Object.assign({}, defaultOptions, settings);

    var initializeOnEvent = function initializeOnEvent() {
      refresh(true);
      window.addEventListener('scroll', throttle(function () {
        handleScroll($elements, $options);
      }, $options.throttleDelay));
      window.addEventListener('resize', debounce(function () {
        refresh();
      }, $options.debounceDelay));
    };

    if ($options.startEvent === 'DOMContentLoaded' && ['complete', 'interactive'].indexOf(document.readyState) > -1) {
      initializeOnEvent();
    } else if ($options.startEvent === 'load') {
      window.addEventListener($options.startEvent, initializeOnEvent);
    } else {
      document.addEventListener($options.startEvent, initializeOnEvent);
    }

    return { refresh: refresh, refreshHard: hardRefresh };
  };

  var aos = {
    init: aosInit,
    refresh: refresh,
    refreshHard: hardRefresh
  };

  return aos;

})));
