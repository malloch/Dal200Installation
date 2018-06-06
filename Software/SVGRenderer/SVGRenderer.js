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
var connectionInterval = 60000;
var trackerData = {};
var targets = {};
var paths = {};
var distThresh = 1000;

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
        socket.onopen = function(event) {
            console.log("Connection established");
            socket.send("SVG renderer "+id+" connected.");
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
            if (data.trackerData) {
                for (var i in data.trackerData) {
                    updateTrackerData(data.trackerData[i].id,
                                      convertCoords(data.trackerData[i].position));
                }
            }
            else if (data.targets) {
                for (var i in data.targets) {
                    updateTarget(data.targets[i].UUID, convertCoords(data.targets[i].Position),
                                 data.targets[i].Label, data.targets[i].Type);
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
//                console.log('dwellIndex:', data.dwellIndex);
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
        switch (e.which) {
            case 32:
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
//
//    updatePath(3, 0, 1);

//    function makeSteps(num) {
//        let steps = [];
//        for (var i = 0; i < num; i++) {
//            let path = canvas.path([['M', 0, 0],
//                                    ['l', 0, 500]])
//                             .attr({'stroke-width': 30,
//                                    'stroke': 'white'})
//                             .rotate(-5)
//                             .translate(220 + i * 40, 165);
//            steps.push(path);
//        }
//    }
//
//    makeSteps(9);

//    // step 1
//    canvas.path([['M', 0, 0],
//                 ['l', 0, 500]])
//          .attr({'stroke-width': 30,
//                 'stroke': Raphael.getColor()})
//          .rotate(-5)
//          .translate(220, 165);
//
//    canvas.circle(300, 500, 40).attr({'fill': 'white',
//                                      'fill-opacity': 1,
//                                      'stroke-width': 10,
//                                      'opacity': 1});
}

function convertCoords(pos) {
    let offset = {'x': 10, 'y': -250};
    let scale = {'x': 2.25, 'y': 2.5};
//    let offset = {'x': 400, 'y': -320};
//    let scale = {'x': 2.4, 'y': 2.5};
//    let offset = {'x': -1110, 'y': -340};
//    let scale = {'x': 1.68, 'y': 2.9};

    let x = pos.x * scale.x + offset.x;
    let y = pos.y * scale.y + offset.y;

    x = 512 - x + 750;

    if (pos.x > 465)
        x -= 45;

    // added y-offset to compensate for projector clamp slipping
    x -= 20;
    y -= 50;

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
//    pos.x = 512 - pos.x + 750;
    if (!trackerData[id]) {
        trackerData[id] = canvas.circle(pos.x, pos.y, 40)
                                .attr({'stroke': 'white',
                                       'fill-opacity': 0,
                                       'stroke-width': 10,
                                       'opacity': 0})
                                .data({'id': id});
        trackerData[id].animationFrame = 0;
    }
    trackerData[id].stop();
//    console.log('placing tracker', id, 'at', pos);
    trackerData[id].animate({'cx': pos.x,
                             'cy': pos.y,
                             'r': trackerData[id].animationFrame, 'opacity': 1},
                           300, 'linear', function() {
        this.animate({'opacity': 0}, 10000, '>', function() {
            trackerData[this.data('id')] = null;
            this.remove();
        });
    });
    if (trackerData[id].animationFrame++ > 40)
        trackerData[id].animationFrame = 20;

    // check if we are close to any targets
    for (var i in targets) {
        let dist = distSquared(pos, targets[i].data('pos'));
//        console.log(id, i, dist);
        targets[i].attr({'stroke-opacity': dist < distThresh ? 1 : 0});
//        if (dist < distThresh) {
////            console.log('tracker', id, 'proximate to target', i);
//            targets[i].attr({'stroke-opacity': 1});
//            if (targets[i].sel < 255)
//                targets[i].sel++;
//        }
//        else if (targets[i].sel > 0)
//            targets[i].sel--;
//        let opacity = targets[i].sel / 255;
//        targets[i].attr({'stroke-opacity': opacity});
    }
}

function updateTarget(id, pos, label, type) {
    if (!targets[id]) {
        targets[id] = canvas.circle(0, 0, 30 + (pos.x) * 0.04);
        targets[id].label = canvas.text(pos.x, pos.y, label)
                                  .rotate(90);
        targets[id].sel = 0;
    }
//    console.log('placing target', id, 'at', pos);
    targets[id].attr({'cx': pos.x, 'cy': pos.y});
    targets[id].data({'pos': pos});
    let color;
    switch (type) {
        case 0:
            color = '#FFBBBB';
            break;
        case 1:
            color = '#BBFFBB';
            break;
        case 2:
            color = '#BBBBFF';
            break;
        default:
            color = '#FFFFFF';
            break;
    }

    targets[id].animate({'fill': color, 'stroke': 'white', 'stroke-opacity': 0, 'stroke-width': 10});
    targets[id].label.animate({'x': pos.x,
                               'y': pos.y,
                               'stroke': 'black',
                               'font-size': 16});
//    console.log(targets);
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
    paths[id].animate({'path': [['M', src.pos.x, src.pos.y],
                                ['S', src.pos.x, dst.pos.y, dst.pos.x, dst.pos.y]],
//                                ['L', dst.pos.x, dst.pos.y]],
                       'stroke': 'white',
                       'stroke-width': 10
                      });
}
