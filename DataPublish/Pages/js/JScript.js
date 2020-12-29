(function (t, e, r) {
    var i = false,
	n = /msie/.test(navigator.userAgent.toLowerCase()),
	o = "//iqiyi.irs01.com/irt?_iwt_id=",
	s = "//irs01.net/MTFlashStore.swf#",
	a = "http://irs01.com/_iwt.gif",
	f = "_iwt_id",
	u = "_iwt_cid",
	c = "",
	h = "",
	l = "",
	d = e.getElementsByTagName("head")[0],
	p = {
	    available: false,
	    guid: function () {
	        return ["MT", (+new Date + v++).toString(36), (Math.random() * 1e18).toString(36)].join("").slice(0, 16).toUpperCase()
	    },
	    get: function (t, e) {
	        return p._sendFlashMsg(e, "jGetItem", t)
	    },
	    set: function (t, e, r) {
	        return p._sendFlashMsg(r, "jSetItem", t, e)
	    },
	    clear: function (t, e) {
	        return p._sendFlashMsg(e, "jClearItem", t)
	    },
	    clearAll: function (t) {
	        return p._sendFlashMsg(t, "jClearAllItems")
	    },
	    _sendFlashMsg: function (e, r, i, n) {
	        e = e || k;
	        var o = p.guid();
	        t[o] = e;
	        switch (arguments.length) {
	            case 2:
	                b[r](o);
	                break;
	            case 3:
	                b[r](i, o);
	                break;
	            case 4:
	                b[r](i, n, o);
	                break
	        }
	    },
	    initSWF: function (t, e) {
	        if (!p.available) {
	            return e && e()
	        }
	        if (p.inited) {
	            return t && setTimeout(t, 0)
	        }
	        t && m.push(t);
	        e && g.push(e)
	    }
	},
	v = 1,
	m = [],
	g = [],
	w = "",
	_,
	b,
	y,
	I = e.createElement("DIV"),
	C = p.guid();
    function k() { }
    function j(e) {
        if (i) {
            if (t.console && t.console.log) {
                t.console.log("MTFlashStore:" + e)
            }
        }
    }
    if (!t._iwt_no_flash) {
        try {
            _ = t.navigator.plugins["Shockwave Flash"] || t.ActiveXObject;
            w = _.description ||
			function () {
			    return new _("ShockwaveFlash.ShockwaveFlash").GetVariable("$version")
			} ()
        } catch (A) { }
        w = (w.match(/\d+/g) || [0])[0];
        if (w < 9) {
            p.available = false
        } else {
            p.available = true;
            t[C] = function (e, r) {
                switch (e) {
                    case "onecall":
                        if (!t[r]) return;
                        t[r].apply(t, [].slice.call(arguments, 2));
                        delete t[r];
                        break;
                    case "error":
                        p.available = false;
                        while (y = g.shift()) {
                            y()
                        }
                        j(r);
                        break;
                    case "load":
                        j("Flash load success!");
                        p.inited = true;
                        p.available = true;
                        while (y = m.shift()) {
                            setTimeout(y, 0)
                        }
                }
            };
            function M() {
                I.setAttribute("style", "display:block;clear:both;float:none;position:absolute;right:0;bottom:0;border:none;");
                if (e.body.firstChild) {
                    e.body.insertBefore(I, e.body.firstChild)
                } else {
                    e.body.appendChild(I)
                }
                I.innerHTML = '<object id="' + p.guid() + (n ? ' classid="clsid:D27CDB6E-AE6D-11cf-96B8-444553540000" ' : "") + '" data="' + s + '" type="application/x-shockwave-flash" ' + ' width="10" height="10" style="position:absolute;right:0;bottom:0;border:none;" >' + '<param name="movie" value="' + s + '" />' + '<param name="wmode" value="transparent" />' + '<param name="version" value="10" />' + '<param name="allowScriptAccess" value="always" />' + '<param name="flashvars" value="jsproxyfunction=' + C + '" />' + "</object>";
                b = I.firstChild;
                p.swf = b
            }
            var S = setInterval(function () {
                if (e.body) {
                    M();
                    clearInterval(S)
                }
            },
			100)
        }
    }
    var O = {
        track: function () {
            if (!p.available) {
                if (this._getQueryStrByName(u) != "") {
                    l = o + "&_iwt_cid=" + this._getQueryStrByName(u) + "&_iwt_UA=" + this._getUAId()
                } else {
                    l = o + "&_iwt_UA=" + this._getUAId()
                }
                x(l,
				function (t) {
				    var e = t;
				    c = t
				})
            } else {
                p.initSWF(T);
                setTimeout(function () {
                    if (!p.inited) {
                        p.available = false;
                        t[C] = k;
                        O.track()
                    }
                },
				3e3)
            }
        },
        record_video_api: function (t, e, r, i) {
            if (i + "" == "0") {
                h = this._getHashId();
                this._feedBack_video(t, e, r, i);
                return
            }
            this._feedBack_video(t, e, r, i)
        },
        _feedBack_video: function (t, e, r, i) {
            var n = ["ta=" + i, "eid=" + h, "pt=" + encodeURIComponent(document.title), "vid=" + t, "du=" + e, "la=" + r, "_iwt_id=" + c, "_iwt_UA=" + this._getUAId(), "r=" + (new Date).getTime()].join("&");
            var o = a + "?" + n;
            this._img = new Image;
            this._img.src = o
        },
        _getHashId: function () {
            var t = location.href,
			e = (new Date).valueOf(),
			r = navigator.userAgent,
			i = "";
            i = this._md5([location.host, t, r, e, Math.random()].join(""));
            return i
        },
        _getUAId: function () {
            if (typeof _iwt_UA == "undefined") {
                return ""
            }
            return _iwt_UA
        },
        _getQueryStrByName: function (t) {
            var e = location.search.match(new RegExp("[?&]" + t + "=([^&]+)", "i"));
            if (e == null || e.length < 1) {
                return ""
            }
            return e[1]
        },
        _getCookie: function (t) {
            var e, r, i, n = document.cookie.split(";");
            for (e = 0; e < n.length; e++) {
                r = n[e].substr(0, n[e].indexOf("="));
                i = n[e].substr(n[e].indexOf("=") + 1);
                r = r.replace(/^\s+|\s+$/g, "");
                if (r == t) {
                    return unescape(i)
                }
            }
        },
        getReferrer: function () {
            var t = "";
            try {
                t = window.top.document.referrer
            } catch (e) {
                if (window.parent) {
                    try {
                        t = window.parent.document.referrer
                    } catch (r) {
                        t = ""
                    }
                }
            }
            if (t == "") {
                t = document.referrer
            }
            return t
        },
        _md5: function (t) {
            function e(t, e) {
                return t << e | t >>> 32 - e
            }
            function r(t, e) {
                var r, i, n, o, s;
                n = t & 2147483648;
                o = e & 2147483648;
                r = t & 1073741824;
                i = e & 1073741824;
                s = (t & 1073741823) + (e & 1073741823);
                if (r & i) {
                    return s ^ 2147483648 ^ n ^ o
                }
                if (r | i) {
                    if (s & 1073741824) {
                        return s ^ 3221225472 ^ n ^ o
                    } else {
                        return s ^ 1073741824 ^ n ^ o
                    }
                } else {
                    return s ^ n ^ o
                }
            }
            function i(t, e, r) {
                return t & e | ~t & r
            }
            function n(t, e, r) {
                return t & r | e & ~r
            }
            function o(t, e, r) {
                return t ^ e ^ r
            }
            function s(t, e, r) {
                return e ^ (t | ~r)
            }
            function a(t, n, o, s, a, f, u) {
                t = r(t, r(r(i(n, o, s), a), u));
                return r(e(t, f), n)
            }
            function f(t, i, o, s, a, f, u) {
                t = r(t, r(r(n(i, o, s), a), u));
                return r(e(t, f), i)
            }
            function u(t, i, n, s, a, f, u) {
                t = r(t, r(r(o(i, n, s), a), u));
                return r(e(t, f), i)
            }
            function c(t, i, n, o, a, f, u) {
                t = r(t, r(r(s(i, n, o), a), u));
                return r(e(t, f), i)
            }
            function h(t) {
                var e;
                var r = t.length;
                var i = r + 8;
                var n = (i - i % 64) / 64;
                var o = (n + 1) * 16;
                var s = Array(o - 1);
                var a = 0;
                var f = 0;
                while (f < r) {
                    e = (f - f % 4) / 4;
                    a = f % 4 * 8;
                    s[e] = s[e] | t.charCodeAt(f) << a;
                    f++
                }
                e = (f - f % 4) / 4;
                a = f % 4 * 8;
                s[e] = s[e] | 128 << a;
                s[o - 2] = r << 3;
                s[o - 1] = r >>> 29;
                return s
            }
            function l(t) {
                var e = "",
				r = "",
				i, n;
                for (n = 0; n <= 3; n++) {
                    i = t >>> n * 8 & 255;
                    r = "0" + i.toString(16);
                    e = e + r.substr(r.length - 2, 2)
                }
                return e
            }
            function d(t) {
                t = t.replace(/\r\n/g, "\n");
                var e = "";
                for (var r = 0; r < t.length; r++) {
                    var i = t.charCodeAt(r);
                    if (i < 128) {
                        e += String.fromCharCode(i)
                    } else if (i > 127 && i < 2048) {
                        e += String.fromCharCode(i >> 6 | 192);
                        e += String.fromCharCode(i & 63 | 128)
                    } else {
                        e += String.fromCharCode(i >> 12 | 224);
                        e += String.fromCharCode(i >> 6 & 63 | 128);
                        e += String.fromCharCode(i & 63 | 128)
                    }
                }
                return e
            }
            var p = Array();
            var v, m, g, w, _, b, y, I, C;
            var k = 7,
			j = 12,
			A = 17,
			M = 22;
            var S = 5,
			O = 9,
			Q = 14,
			F = 20;
            var N = 4,
			T = 11,
			x = 16,
			E = 23;
            var U = 6,
			D = 10,
			q = 15,
			R = 21;
            t = d(t);
            p = h(t);
            b = 1732584193;
            y = 4023233417;
            I = 2562383102;
            C = 271733878;
            for (v = 0; v < p.length; v += 16) {
                m = b;
                g = y;
                w = I;
                _ = C;
                b = a(b, y, I, C, p[v + 0], k, 3614090360);
                C = a(C, b, y, I, p[v + 1], j, 3905402710);
                I = a(I, C, b, y, p[v + 2], A, 606105819);
                y = a(y, I, C, b, p[v + 3], M, 3250441966);
                b = a(b, y, I, C, p[v + 4], k, 4118548399);
                C = a(C, b, y, I, p[v + 5], j, 1200080426);
                I = a(I, C, b, y, p[v + 6], A, 2821735955);
                y = a(y, I, C, b, p[v + 7], M, 4249261313);
                b = a(b, y, I, C, p[v + 8], k, 1770035416);
                C = a(C, b, y, I, p[v + 9], j, 2336552879);
                I = a(I, C, b, y, p[v + 10], A, 4294925233);
                y = a(y, I, C, b, p[v + 11], M, 2304563134);
                b = a(b, y, I, C, p[v + 12], k, 1804603682);
                C = a(C, b, y, I, p[v + 13], j, 4254626195);
                I = a(I, C, b, y, p[v + 14], A, 2792965006);
                y = a(y, I, C, b, p[v + 15], M, 1236535329);
                b = f(b, y, I, C, p[v + 1], S, 4129170786);
                C = f(C, b, y, I, p[v + 6], O, 3225465664);
                I = f(I, C, b, y, p[v + 11], Q, 643717713);
                y = f(y, I, C, b, p[v + 0], F, 3921069994);
                b = f(b, y, I, C, p[v + 5], S, 3593408605);
                C = f(C, b, y, I, p[v + 10], O, 38016083);
                I = f(I, C, b, y, p[v + 15], Q, 3634488961);
                y = f(y, I, C, b, p[v + 4], F, 3889429448);
                b = f(b, y, I, C, p[v + 9], S, 568446438);
                C = f(C, b, y, I, p[v + 14], O, 3275163606);
                I = f(I, C, b, y, p[v + 3], Q, 4107603335);
                y = f(y, I, C, b, p[v + 8], F, 1163531501);
                b = f(b, y, I, C, p[v + 13], S, 2850285829);
                C = f(C, b, y, I, p[v + 2], O, 4243563512);
                I = f(I, C, b, y, p[v + 7], Q, 1735328473);
                y = f(y, I, C, b, p[v + 12], F, 2368359562);
                b = u(b, y, I, C, p[v + 5], N, 4294588738);
                C = u(C, b, y, I, p[v + 8], T, 2272392833);
                I = u(I, C, b, y, p[v + 11], x, 1839030562);
                y = u(y, I, C, b, p[v + 14], E, 4259657740);
                b = u(b, y, I, C, p[v + 1], N, 2763975236);
                C = u(C, b, y, I, p[v + 4], T, 1272893353);
                I = u(I, C, b, y, p[v + 7], x, 4139469664);
                y = u(y, I, C, b, p[v + 10], E, 3200236656);
                b = u(b, y, I, C, p[v + 13], N, 681279174);
                C = u(C, b, y, I, p[v + 0], T, 3936430074);
                I = u(I, C, b, y, p[v + 3], x, 3572445317);
                y = u(y, I, C, b, p[v + 6], E, 76029189);
                b = u(b, y, I, C, p[v + 9], N, 3654602809);
                C = u(C, b, y, I, p[v + 12], T, 3873151461);
                I = u(I, C, b, y, p[v + 15], x, 530742520);
                y = u(y, I, C, b, p[v + 2], E, 3299628645);
                b = c(b, y, I, C, p[v + 0], U, 4096336452);
                C = c(C, b, y, I, p[v + 7], D, 1126891415);
                I = c(I, C, b, y, p[v + 14], q, 2878612391);
                y = c(y, I, C, b, p[v + 5], R, 4237533241);
                b = c(b, y, I, C, p[v + 12], U, 1700485571);
                C = c(C, b, y, I, p[v + 3], D, 2399980690);
                I = c(I, C, b, y, p[v + 10], q, 4293915773);
                y = c(y, I, C, b, p[v + 1], R, 2240044497);
                b = c(b, y, I, C, p[v + 8], U, 1873313359);
                C = c(C, b, y, I, p[v + 15], D, 4264355552);
                I = c(I, C, b, y, p[v + 6], q, 2734768916);
                y = c(y, I, C, b, p[v + 13], R, 1309151649);
                b = c(b, y, I, C, p[v + 4], U, 4149444226);
                C = c(C, b, y, I, p[v + 11], D, 3174756917);
                I = c(I, C, b, y, p[v + 2], q, 718787259);
                y = c(y, I, C, b, p[v + 9], R, 3951481745);
                b = r(b, m);
                y = r(y, g);
                I = r(I, w);
                C = r(C, _)
            }
            var G = l(b) + l(y) + l(I) + l(C);
            return G.toLowerCase()
        }
    },
	Q,
	F = false,
	N = false;
    function T() {
        p.get(f,
		function (t) {
		    if (!t) {
		        if (O._getQueryStrByName(u) != "") {
		            l = o + "&_iwt_cid=" + O._getQueryStrByName(u) + "&_iwt_UA=" + O._getUAId()
		        } else {
		            l = o + "&_iwt_UA=" + O._getUAId()
		        }
		        x(l,
				function (t) {
				    p.set(f, t);
				    c = t
				})
		    } else {
		        if (O._getQueryStrByName(u) != "") {
		            l = o + t + "&_iwt_cid=" + O._getQueryStrByName(u) + "&_iwt_UA=" + O._getUAId()
		        } else {
		            l = o + t + "&_iwt_UA=" + O._getUAId()
		        }
		        x(l,
				function (t) {
				    p.set(f, t);
				    c = t
				})
		    }
		})
    }
    function x(i, n) {
        var o = e.createElement("script"),
		s = p.guid();
        t[s] = function () {
            try {
                n.apply(t, arguments);
                d.removeChild(o)
            } catch (e) { }
            t[s] = r
        };
        o.setAttribute("type", "text/javascript");
        o.setAttribute("charset", "utf-8");
        var a = i + "&jsonp=" + s + "&";
        if (typeof _iwt_p1 == "undefined") {
            a = a + "_iwt_p1=&"
        } else {
            a = a + "_iwt_p1=" + _iwt_p1 + "&"
        }
        if (typeof _iwt_p2 == "undefined") {
            a = a + "_iwt_p2=&"
        } else {
            a = a + "_iwt_p2=" + _iwt_p2 + "&"
        }
        if (typeof _iwt_p3 == "undefined") {
            a = a + "_iwt_p3=&"
        } else {
            a = a + "_iwt_p3=" + _iwt_p3 + "&"
        }
        if (typeof _iwt_no_referrer != "undefined" && !_iwt_no_referrer) {
            var f = O.getReferrer();
            if (f != "") {
                a = a + "ref=" + encodeURIComponent(f) + "&"
            }
        }
        o.setAttribute("src", a);
        if (d.firstChild) {
            d.insertBefore(o, d.firstChild)
        } else {
            d.appendChild(o)
        }
    }
    t._iwt = O;
    O.FC = p;
    _iwt.track()
})(window, document);