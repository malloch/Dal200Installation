"use strict"

window.onload = init;

var data = [];
var socket = null;
var connected = false;
var connectionInterval = 60000;
var contentIdx = -1;
var globe = null;

var locations = [];
var pingColor = 'white';
var pingInterval = 1000;

var files = ["lovelace",    "lamarr",       "hopper",       "keller",
             "hamilton",    "wss",          "suresh",       "wv",
             "watters",     "jutla",        "bahr-gedalia", "zincir-heywood",
             "alshazly",    "perry",        "orji",         "worsley",
             "tu",          "klawe",        "payette",      "condon",
             "murphy",      "reid",         "xiao",         "cannon" ];

function init() {
    console.log('init!');

    (function() {
        globe = planetaryjs.planet();
        // Load our custom `autorotate` plugin; see below.
        globe.loadPlugin(autorotate(10));
        // The `earth` plugin draws the oceans and the land; it's actually
        // a combination of several separate built-in plugins.
        //
        // Note that we're loading a special TopoJSON file
        // (world-110m-withlakes.json) so we can render lakes.
        globe.loadPlugin(planetaryjs.plugins.earth({
            topojson: { file:   '../Includes/world-110m.json' },
                        oceans:   { fill:   '#000080' },
                        land:     { fill:   '#339966' },
                        borders:  { stroke: '#008000' }
        }));

        var canvas = document.getElementById('rotatingGlobe');
        globe.projection
             .scale(200)
             .translate([200, 200]);

        // The `pings` plugin draws animated pings on the globe.
        globe.loadPlugin(planetaryjs.plugins.pings());

        // Draw that globe!
        globe.draw(canvas);



        // This plugin will automatically rotate the globe around its vertical
        // axis a configured number of degrees every second.
        function autorotate(degPerSec) {
            // Planetary.js plugins are functions that take a `planet` instance
            // as an argument...
            return function(planet) {
                var lastTick = null;
                var paused = false;
                planet.plugins.autorotate = {
                    pause:  function() { paused = true;  },
                    resume: function() { paused = false; }
                };
                // ...and configure hooks into certain pieces of its lifecycle.
                planet.onDraw(function() {
                    if (paused || !lastTick) {
                        lastTick = new Date();
                    } else {
                        var now = new Date();
                        var delta = now - lastTick;
                        // This plugin uses the built-in projection (provided by D3)
                        // to rotate the globe each time we draw it.
                        var rotation = planet.projection.rotate();
                        rotation[0] += degPerSec * delta / 1000;
                        if (rotation[0] >= 180) rotation[0] -= 360;
                            planet.projection.rotate(rotation);
                        lastTick = now;
                    }
                });
            };
        };
    })();

    if (navigator.onLine) {
        console.log("You are Online");
    }
    else {
        console.log("You are Offline");
    }

    function openWebSocket() {
        console.log("   Trying to connect...");
        // Create a new WebSocket.
        socket = new WebSocket('ws://192.168.1.120/Dal200');
        socket.onopen = function(event) {
            console.log("Connection established");
            socket.send("Globe renderer "+id+" connected.");
            connected = true;
        }
        socket.onclose = function(event) {
            console.log("Connection dropped, will try to reconnect in", connectionInterval/1000, "seconds");
            connected = false;
        }
        socket.onmessage = function(event) {
            let data = null;
            if (event.data)
                data = JSON.parse(event.data);
            if (!data)
                return;
            if (data.dwellIndex != null) {
                console.log('dwellIndex:', data.dwellIndex);
                chooseIndex(data.dwellIndex);
            }
            else if (data.screenSaver) {
                // start screensaver
            }
        }
    }

    // open webSocket
    openWebSocket();

    // check the websocket periodically
    setInterval(function() {
        console.log("checking websocket...");
        if (connected == true) {
            console.log("   Socket ok.");
            return;
        }
        openWebSocket();
    }, connectionInterval);

    setInterval(function() {
        for (var i in locations) {
            let loc = locations[i];
            globe.plugins.pings.add(loc.lng, loc.lat, { color: pingColor,
                                                        ttl: 4000,
                                                        angle: 10 });
        }
    }, pingInterval);

    // load json data (positions only)
    files.forEach(function(name) {
        $.getJSON("../../Content/data/"+name+".json", function(_data) {
            let idx = files.indexOf(_data.key);
            console.log("adding "+_data.key+" at index "+idx);
            data[idx] = _data.locations;
        });
    });

    function chooseIndex(idx) {
        console.log("chooseIndex("+idx+")");
        let keys = Object.keys(data);
        let entry = data[keys[idx]];
        console.log("Loading content["+contentIdx+"] :", keys[contentIdx]);

        if (entry) {
            locations = [];
            for (var i in entry) {
                console.log("loading location", entry[i], "for", keys[idx]);
                let loc = entry[i];
                if (!loc.lng || !loc.lat)
                    continue;
                locations.push(loc);
            }
        }
    }

    $('body').on('keydown.list', function(e) {
        console.log("keypress:", e.which);
        switch (e.which) {
            case 78:
                /* 'N' */
                // dismiss screensaver if necessary
//                if (screensaver) {
//                    d3.select('#ifmContent').attr('src', "");
//                    screensaver = 0;
//                }
                // load the next entry
                let keys = Object.keys(data);
                contentIdx++;
                if (contentIdx > (keys.length -1))
                    contentIdx = 0;
                chooseIndex(contentIdx);
                break;
            default:
                console.log("keypress:", e.which);
        }
    });
}
