"use strict"

window.onload = init;

var id = 1;
var canvas;
var width;
var height;
var cx;
var cy;
var socket = null;
var connected = false;
var connectionInterval = 30000;
var trackerData = {};
var targets = {};
var paths = {};

var trackerHistory = [];
var trail = null;
var links = [];

var distThresh = 10000;

var screensaver = false;
var screensaver_timeout = 30000;

function init() {
    console.log('init!');

    canvas = Raphael($('#svgDiv')[0], '100%', '100%');
    width = ($('#svgDiv')[0]).offsetWidth;
    height = ($('#svgDiv')[0]).offsetHeight;
    console.log('window dimensions: '+width+'x'+height);
    cx = width * 0.5;
    cy = height * 0.5;

//    identify();

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
            socket.send("SVG renderer "+id+" connected.");
            connected = true;
        }
        socket.onclose = function(event) {
            console.log("Connection dropped, will try to reconnect in", connectionInterval/1000, "seconds");
            connected = false;
//            for (var i in targets) {
//                targets[i].label.remove();
//                targets[i].remove();
//                targets[i] = null;
//            }
//            targets = {};
        }
        socket.onmessage = function(event) {
            let data = null;
            if (event.data)
                data = JSON.parse(event.data);
            if (!data)
                return;
            if (data.trackerData) {
                for (var i in data.trackerData) {
                    updateTrackerData(data.trackerData[i].id,
                                      convertCoords(data.trackerData[i].position));
//                    console.log(data.trackerData[i].position);
                }
                if (screensaver == true) {
                    screensaver = false;
                    

                    // animate targets back to their proper positions
                    for (var i in targets) {
                    	//Stop the current animations first
                    	targets[i].stop();
                        let pos = targets[i].data('pos')
                        targets[i].stop()
                                  .animate({'cx': pos.x,
                                            'cy': pos.y,
                                            'opacity': 1}, 1000, 'linear', function() {
                            this.label.stop()
                                      .animate({'opacity': 1}, 500, 'linear');
                        });
                    }
                }
            }
            else if (data.targets) {
                for (var i in data.targets) {
                    updateTarget(data.targets[i].UUID, convertCoords(data.targets[i].Position),
                                 data.targets[i].Label, data.targets[i].Type,
                                 data.targets[i].Page);
                }
            }
            else if (data.paths) {
                for (var i in data.paths) {
                    updatePath(data.paths[i].UUID, data.paths[i].source,
                               data.paths[i].destination);
                }
            }
            else if (data.command) {
                switch (data.command) {
                    case 'identify':
                        identify();
                        break;
                }
            }
            else if (data.dwellIndex != null) {
                // find target with dwell index
                for (var i in targets) {
                    if (targets[i].dwellIndex == data.dwellIndex) {
                        targets[i].attr({'stroke': 'red'})
                                  .animate({'stroke': 'white'}, 2000, 'linear');
                        break;
                    }
                }
            }
            else if (data.screenSaver) {
                startScreenSaver();
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

    // open webSocket
    openWebSocket();

    // activate the screensaver if necessary
    setInterval(function() {
        console.log("checking for activity...");
        if (screensaver != true) {
            console.log("animating!");
            startScreenSaver();
        }
        screensaver = true;
    }, screensaver_timeout);

    $('body').on('keydown.list', function(e) {
        switch (e.which) {
            case 32:
                 socket.close();
                 break;
                /* space */
                for (var i in trackerData) {
                    updateTrackerData(i, randomCoord());
                }
                for (var i in targets) {
                    updateTarget(i, randomCoord());
                }
                break;
        }
    })

    // debugging: add a couple of targets
//    updateTarget(0, randomCoord(), "category 1", 1);
//    updateTarget(1, randomCoord(), "category 2", 0);
//    updateTarget(2, randomCoord(), "category 3", 2);
//    updateTarget(3, randomCoord(), "category 1", 1);
//    updateTarget(4, randomCoord(), "category 2", 0);
//    updateTarget(5, randomCoord(), "category 3", 2);
//    updateTarget(6, randomCoord(), "category 1", 1);
//    updateTarget(7, randomCoord(), "category 2", 0);
//    updateTarget(8, randomCoord(), "category 3", 2);
//    updateTarget(9, randomCoord(), "category 1", 1);
//    updateTarget(10, randomCoord(), "category 2", 0);
//    updateTarget(11, randomCoord(), "category 3", 2);
//    updateTarget(12, randomCoord(), "category 1", 1);
//    updateTarget(13, randomCoord(), "category 2", 0);
//    updateTarget(14, randomCoord(), "category 3", 2);
//    updateTarget(15, randomCoord(), "category 1", 1);
//    updateTarget(16, randomCoord(), "category 2", 0);
//    updateTarget(17, randomCoord(), "category 3", 2);
//    updateTarget(18, randomCoord(), "category 1", 1);
//    updateTarget(19, randomCoord(), "category 2", 0);
//    updateTarget(20, randomCoord(), "category 3", 2);
//    updateTarget(21, randomCoord(), "category 1", 1);
//    updateTarget(22, randomCoord(), "category 2", 0);
//    updateTarget(23, randomCoord(), "category 3", 2);
}

function startScreenSaver() {
    links = [];
    for (var i in targets) {
        links[i] = [];
        for (var j in targets) {
            if (i == j)
                continue;
            links[i][j] = canvas.path().attr({'stroke': 'white',
                                              'stroke-width': 1})
                                       .toBack();
        }
    }

    let distmult = 0.00002;

    function a(o) {
        let cx = o.attr("cx");
        let cy = o.attr("cy");
        let newPos = randomCoord();
        $("<div></div>")
        .css({'x': cx,
              'y': cy})
        .animate({'x': newPos.x,
                  'y': newPos.y},
                 {duration : Math.random() * 10000 + 10000,
                  complete : function() { b(o); },
                  step : function(now, fx) {
            if (fx.prop == 'x')
                o.attr("cx", now );
            if (fx.prop == 'y') {
                o.attr("cy", now );
                let i = o.index;
                for (var j = i + 1; j < Object.keys(targets).length; j++) {
                    // animate line
                    let dist = distSquared({'x': o.attr('cx'),
                                            'y': o.attr('cy')},
                                           {'x': targets[j].attr('cx'),
                                            'y': targets[j].attr('cy')});
                    dist = dist * distmult;
                    if (dist >= 1)
                        links[i][j].attr({'opacity': 0});
                    else
                        links[i][j].attr({'path': [['M', o.attr('cx'), o.attr('cy')],
                                                   ['L', targets[j].attr('cx'),
                                                    targets[j].attr('cy')]],
                                          'opacity': 1 - dist});
                }
            }
        }});
    }

    function b(o) {
        let cx = o.attr("cx");
        let cy = o.attr("cy");
        let newPos = randomCoord();
        $("<div></div>")
        .css({'x': cx,
              'y': cy})
        .animate({'x': newPos.x,
                  'y': newPos.y},
                  {duration : Math.random() * 10000 + 10000,
                   complete : function() { a(o); },
                   step : function(now, fx) {
            if (fx.prop == 'x')
                o.attr("cx", now );
            if (fx.prop == 'y') {
                o.attr("cy", now );
                let i = o.index;
                for (var j = i + 1; j < Object.keys(targets).length; j++) {
                    // animate line
                    let dist = distSquared({'x': o.attr('cx'),
                                            'y': o.attr('cy')},
                                           {'x': targets[j].attr('cx'),
                                            'y': targets[j].attr('cy')});
                    dist = dist * distmult;
                    if (dist >= 1)
                        links[i][j].attr({'opacity': 0});
                    else
                        links[i][j].attr({'path': [['M', o.attr('cx'), o.attr('cy')],
                                                   ['L', targets[j].attr('cx'),
                                                    targets[j].attr('cy')]],
                                          'opacity': 1 - dist});
                }
            }
        }});
    }

    for (var i in targets) {
        targets[i].label.animate({'opacity': 0}, 500, 'linear');
        targets[i].animate({'opacity': 0.0}, 2000);
        a(targets[i]);
    }
}

function stopScreenSaver() {
    for (var i in links) {
        for (var j in links[i]) {
            links[i][j].remove();
        }
    }
}

function convertCoords(pos) {
    let offset = {'x': -50, 'y': 820};
    let scale = {'x': 2.3, 'y': -2.5};

    let x = pos.x * scale.x + offset.x;
    let y = pos.y * scale.y + offset.y;

    return {'x': x, 'y': y};
}

function identify() {
    let test = canvas.text(cx, cy, 'Dal200 SVG Renderer #'+id)
                     .attr({'opacity': 1,
                            'font-size': 36,
                            'fill': 'white'})
                     .animate({'opacity': 0}, 10000, 'linear', function() {
                              this.remove();
                              });
}

function randomCoord() {
    return {'x': Math.random() * width, 'y': Math.random() * height};
}

function circlePath(pos, r1, r2, a) {
    if (!r2)
        r2 = r1;
    if (!a)
        a = 0;
    return [['M', pos.x + r1 * 0.65, pos.y - r2 * 0.65],
            ['a', r1, r2, a, 1, 0, 0.001, 0.001],
            ['z']];
}

function distSquared(pos1, pos2) {
    let distX = pos1.x - pos2.x;
    let distY = pos1.y - pos2.y;
    return distX * distX + distY * distY;
}

function updateTrackerData(id, pos) {
    if (!trackerData[id]) {
        trackerData[id] = canvas.circle(pos.x, pos.y, 40)
                                .attr({'stroke': 'white',
                                       'fill-opacity': 0,
                                       'stroke-width': 10,
                                       'opacity': 0})
                                .data({'id': id});
        trackerData[id].animationFrame = 0;
    }
    trackerData[id].stop().toFront();
    trackerData[id].animate({'cx': pos.x,
                             'cy': pos.y,
                             'r': trackerData[id].animationFrame, 'opacity': 1},
                           300, 'linear', function() {
        this.animate({'opacity': 0}, 10000, '>', function() {
            trackerData[this.data('id')] = null;
            this.remove();
        });
    });
    trackerData[id].animationFrame += 0.5;
    if (trackerData[id].animationFrame > 40)
        trackerData[id].animationFrame = 20;

    // check if we are close to any targets
    for (var i in targets) {
        let dist = distSquared(pos, targets[i].data('pos'));
        targets[i].attr({'stroke-opacity': dist < distThresh ? 1 : 0,
                         'stroke-width': dist < distThresh ? (distThresh - dist) * 0.001 : 0
                        });
    }
    
//    // draw a trail
//    trackerHistory.push(pos);
//    while (trackerHistory.length > 30)
//        trackerHistory.shift();
//    if (trackerHistory.length > 1) {
//        let path = [];
//        path.push(['M', trackerHistory[0].x, trackerHistory[0].y]);
//        for (var i = 0; i < trackerHistory.length; i++)
//            path.push(['T', trackerHistory[i].x, trackerHistory[i].y]);
//        if (!trail)
//            trail = canvas.path().attr({'stroke': 'white',
//                                        'stroke-width': 25,
//                                        'opacity': 0,
//                                        'stroke-linecap': 'round'});
//        trail.stop().animate({'path': path, 'opacity': 0.7}, 100, 'linear', function() {
//            this.animate({'opacity': 0}, 2000, 'linear', function()  {
//                while (trackerHistory.length)
//                    trackerHistory.shift();
//            });
//        }).toBack();
//    }
}

function updateTarget(id, pos, label, type, dwellIndex) {
//    switch (label) {
//        case "Lovelace":
//            pos.x = 65;
//            pos.y = 205;
//            break;
//        case "Hopper":
//            pos.x = 65;
//            pos.y = 505;
//            break;
//        case "Lamar":
//            pos.x = 115;
//            pos.y = 355;
//            break;
//        case "Keller":
//            pos.x = 175;
//            pos.y = 205;
//            break;
//        case "Hamilton":
//            pos.x = 175;
//            pos.y = 505;
//            break;
//        case "W-S-S":
//            pos.x = 225;
//            pos.y = 355;
//            break;
//        case "Suresh":
//            pos.x = 295;
//            pos.y = 205;
//            break;
//        case "Worsley":
//            pos.x = 295;
//            pos.y = 455;
//            break;
//        case "Yu":
//            pos.x = 365;
//            pos.y = 305;
//            break;
//        case "Klawe":
//            pos.x = 445;
//            pos.y = 205;
//            break;
//        case "Payette":
//            pos.x = 445;
//            pos.y = 505;
//            break;
//        case "Condon":
//            pos.x = 525;
//            pos.y = 355;
//            break;
//        case "Nur":
//            pos.x = 655;
//            pos.y = 205;
//            break;
//        case "Watters":
//            pos.x = 655;
//            pos.y = 555;
//            break;
//        case "Orji":
//            pos.x = 705;
//            pos.y = 355;
//            break;
//    }
    if (!targets[id]) {
        targets[id] = canvas.circle(0, 0, 50);
        targets[id].label = canvas.text(pos.x, pos.y, label)
                                  .rotate(90);
        targets[id].sel = 0;
        targets[id].dwellIndex = dwellIndex;
        targets[id].index = id;
    }

    targets[id].attr({'cx': pos.x, 'cy': pos.y});
    targets[id].data({'pos': pos});
    let color;
    switch (type) {
        case 0:
            color = Raphael.hsl(0.1, 1, 0.6);
            break;
        case 1:
            color = Raphael.hsl(0.5, 1, 0.5);
            break;
        case 2:
            color = Raphael.hsl(0.9, 1, 0.5);
            break;
        default:
            color = '#FFFFFF';
            break;
    }
    
    targets[id].stop()
               .animate({'fill': color,
                         'stroke': 'white',
                         'stroke-opacity': 0,
                         'stroke-width': 10}).toBack();
    targets[id].label.stop()
                     .animate({'x': pos.x,
                               'y': pos.y,
                               'fill': 'white',
                               'font-size': 24});
}

function updatePath(id, src, dst) {
    src = targets[src];
    dst = targets[dst];
    if (!src) {
        console.log('missing src target for path', id);
        return;
    }
    if (!dst) {
        console.log('missing dst target for path', id);
        return;
    }
    if (!paths[id]) {
        console.log('adding path');
        paths[id] = canvas.path([['M', src.pos.x, src.pos.y]])
                          .attr({'stroke-dasharray': '.'})
                          .toBack();
    }
    paths[id].stop()
             .animate({'path': [['M', src.pos.x, src.pos.y],
                                ['S', src.pos.x, dst.pos.y, dst.pos.x, dst.pos.y]],
//                              ['L', dst.pos.x, dst.pos.y]],
                       'stroke': 'white',
                       'stroke-width': 10
    });
}
