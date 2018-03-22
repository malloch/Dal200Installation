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
//            console.log(data);
            for (var i in data.targets) {
                updateTarget(data.targets[i].UUID, data.targets[i].Position,
                             data.targets[i].Label);
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
                break;
        }
    })
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

function randomCoord(num) {
    if (!num || num < 2)
        return [Math.random() * width, Math.random() * height];
    return {'x': randomCoord(1), 'y': randomCoord(1)};
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

function updateTrackerData(id, pos) {
    if (!trackerData[id]) {
        trackerData[id] = canvas.path([['M', pos.x, pos.y]])
                                .attr({'stroke': 'red',
                                       'fill': 'red',
                                       'opacity': 0})
        .data({'id': id});
    }
    let path = circlePath(pos, 10);
    trackerData[id].stop();
    trackerData[id].animate({'path': path, 'opacity': 1},
                           300, 'linear', function() {
        this.animate({'opacity': 0}, 10000, '>', function() {
            trackerData[this.data('id')] = null;
            this.remove();
        });
    });
}

function updateTarget(id, pos, label) {
    if (!targets[id]) {
        targets[id] = canvas.path([['M', pos.x, pos.y]]);
        targets[id].label = canvas.text(pos.x, pos.y, label);
    }
    targets[id].animate({'path': circlePath(pos, 50),
                        'fill': 'white'});
    targets[id].label.animate({'x': pos.x,
                              'y': pos.y,
                              'stroke': 'black',
                              'font-size': 16});
//    console.log(targets);
}


