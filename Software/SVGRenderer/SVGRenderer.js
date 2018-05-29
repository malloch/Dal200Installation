"use strict"

window.onload = init;

var id = 1;
var canvas;
var width;
var height;
var cx;
var cy;
var socket = null;
var trackerData = {};
var targets = {};
var paths = {};
var distThresh = 5000;

function init() {
    console.log('init!');

    canvas = Raphael($('#svgDiv')[0], '100%', '100%');
    width = ($('#svgDiv')[0]).offsetWidth;
    height = ($('#svgDiv')[0]).offsetHeight;
    console.log('window dimensions: '+width+'x'+height);
    cx = width * 0.5;
    cy = height * 0.5;

    identify();

    if (navigator.onLine) {
        console.log("You are Online");
    } else {
        console.log("You are Offline");
    }

    // Create a new WebSocket.
    socket = new WebSocket('ws://134.190.155.223/Dal200');
//    socket = new WebSocket('ws://192.168.0.107/Dal200');
    socket.onopen = function(event) {
        console.log("Connection established");
        socket.send("SVG renderer "+id+" connected.");
    }
    socket.onmessage = function(event) {
//        console.log("message received:", event);
        let data = null;
        if (event.data)
            data = JSON.parse(event.data);
        if (!data)
            return;
//        console.log(data);
        if (data.trackerData) {
            for (var i in data.trackerData) {
                updateTrackerData(data.trackerData[i].id,
                                  data.trackerData[i].position);
            }
        }
        else if (data.targets) {
            console.log(data);
            for (var i in data.targets) {
                updateTarget(data.targets[i].UUID, data.targets[i].Position,
                             data.targets[i].Label);
            }
        }
        else if (data.paths) {
            console.log('paths:', data.paths);
            for (var i in data.paths) {
                updatePath(data.paths[i].UUID, data.paths[i].source,
                           data.paths[i].destination);
            }
        }
        else if (data.command) {
            console.log(data);
            switch (data.command) {
                case 'identify':
                    identify();
                    break;
            }
        }
        else {
            console.log('unknown message:', data);
        }
    }

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
    updateTarget(0, randomCoord(), "category 1");
    updateTarget(1, randomCoord(), "category 2");

    updatePath(3, 0, 1);
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
    trackerData[id].stop();
    trackerData[id].animate({'r': trackerData[id].animationFrame, 'opacity': 1},
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
        if (dist < distThresh) {
            console.log('tracker', id, 'proximate to target', i);
            if (targets[i].sel < 255)
                targets[i].sel++;
        }
        else if (targets[i].sel > 0)
            targets[i].sel--;
        let color = targets[i].sel;
        targets[i].attr({'fill': Raphael.rgb(255, 255-color, 255-color)});
    }
}

function updateTarget(id, pos, label) {
    if (!targets[id]) {
        targets[id] = canvas.path([['M', pos.x, pos.y]]);
        targets[id].label = canvas.text(pos.x, pos.y, label)
                                  .rotate(-90);
        targets[id].sel = 0;
    }
    targets[id].pos = pos;
    targets[id].data({'pos': pos});
    targets[id].animate({'path': circlePath(pos, 50),
                         'fill': 'white'});
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
