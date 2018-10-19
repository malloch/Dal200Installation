"use strict"

window.onload = init;

var id = 1;
var socket = null;
var connected = false;
var connectionInterval = 60000;

function init() {
    console.log('init!');

//    (function() {
//        var canvas = document.getElementById('basicGlobe');
//        var planet = planetaryjs.planet();
//        // Loading this plugin technically happens automatically,
//        // but we need to specify the path to the `world-110m.json` file.
//        planet.loadPlugin(planetaryjs.plugins.earth({
//            topojson: { file: '../Includes/world-110m.json' }
//        }));
//        // Scale the planet's radius to half the canvas' size
//        // and move it to the center of the canvas.
//        planet.projection
//              .scale(canvas.width / 2)
//              .translate([canvas.width / 2, canvas.height / 2]);
//        planet.draw(canvas);
//    })();

    (function() {
        var globe = planetaryjs.planet();
        // Load our custom `autorotate` plugin; see below.
        globe.loadPlugin(autorotate(10));
        // The `earth` plugin draws the oceans and the land; it's actually
        // a combination of several separate built-in plugins.
        //
        // Note that we're loading a special TopoJSON file
        // (world-110m-withlakes.json) so we can render lakes.
        globe.loadPlugin(planetaryjs.plugins.earth({
            topojson: { file:   '../Includes/world-110m-withlakes.json' },
                        oceans:   { fill:   '#000080' },
                        land:     { fill:   '#339966' },
                        borders:  { stroke: '#008000' }
        }));
        // Load our custom `lakes` plugin to draw lakes; see below.
        globe.loadPlugin(lakes({
            fill: '#000080'
        }));
        // The `pings` plugin draws animated pings on the globe.
        globe.loadPlugin(planetaryjs.plugins.pings());
        // The `zoom` and `drag` plugins enable
        // manipulating the globe with the mouse.
        globe.loadPlugin(planetaryjs.plugins.zoom({
            scaleExtent: [100, 300]
        }));
        globe.loadPlugin(planetaryjs.plugins.drag({
            // Dragging the globe should pause the
            // automatic rotation until we release the mouse.
            onDragStart: function() {
                this.plugins.autorotate.pause();
            },
            onDragEnd: function() {
                this.plugins.autorotate.resume();
            }
        }));
        // Set up the globe's initial scale, offset, and rotation.
        globe.projection.scale(175).translate([175, 175]).rotate([0, -10, 0]);

        // Every few hundred milliseconds, we'll draw another random ping.
//        var colors = ['red', 'yellow', 'white', 'orange', 'green', 'cyan', 'pink'];
//        setInterval(function() {
//            var lat = Math.random() * 170 - 85;
//            var lng = Math.random() * 360 - 180;
//            var color = colors[Math.floor(Math.random() * colors.length)];
//            globe.plugins.pings.add(lng, lat, { color: color, ttl: 2000, angle: Math.random() * 10 });
//        }, 150);

        var canvas = document.getElementById('rotatingGlobe');
        console.log("canvas:", canvas);
        // Special code to handle high-density displays (e.g. retina, some phones)
        // In the future, Planetary.js will handle this by itself (or via a plugin).
        if (window.devicePixelRatio == 2) {
            canvas.width = 800;
            canvas.height = 800;
            let context = canvas.getContext('2d');
            context.scale(2, 2);
        }
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

        // This plugin takes lake data from the special
        // TopoJSON we're loading and draws them on the map.
        function lakes(options) {
            options = options || {};
            var lakes = null;

            return function(planet) {
                planet.onInit(function() {
                    // We can access the data loaded from the TopoJSON plugin
                    // on its namespace on `planet.plugins`. We're loading a custom
                    // TopoJSON file with an object called "ne_110m_lakes".
                    var world = planet.plugins.topojson.world;
                    lakes = topojson.feature(world, world.objects.ne_110m_lakes);
                });

                planet.onDraw(function() {
                    planet.withSavedContext(function(context) {
                        context.beginPath();
                        planet.path.context(context)(lakes);
                        context.fillStyle = options.fill || 'black';
                        context.fill();
                    });
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
//        socket = new WebSocket('ws://134.190.133.142/Dal200');
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
//                console.log('dwellIndex:', data.dwellIndex);
                // find target with dwell index
                // look up location info for this target
                // rotate globe and label location
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

    $('body').on('keydown.list', function(e) {
        console.log("keypress:", e.which);
        switch (e.which) {
            case 32:
                /* space */
                break;
        }
    })
}
