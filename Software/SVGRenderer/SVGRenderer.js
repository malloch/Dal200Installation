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
var nodes = {};

function init() {
    console.log('init!');

    canvas = Raphael($('#svgDiv')[0], '100%', '100%');
    width = ($('#svgDiv')[0]).offsetWidth;
    height = ($('#svgDiv')[0]).offsetHeight;
    cx = width * 0.5;
    cy = height * 0.5;

    let test = canvas.text(cx, cy, 'Dal200 SVG Renderer #'+id)
                     .attr({'opacity': 0,
                            'font-size': 36,
                            'fill': 'white'})
                     .animate({'opacity': 1}, 10000, 'linear',
                              function() {
                                this.remove();
                              });

    if (navigator.onLine) {
        console.log("You are Online");
    } else {
        console.log("You are Offline");
    }

    // Create a new WebSocket.
    socket = new WebSocket('ws://134.190.132.64/Dal200');
    socket.onopen = function(event) {
        console.log("Connection established");
        socket.send("a message from Joe");
    }
    socket.onmessage = function(event) {
        console.log("message received:", event);
    }

    updateTrackerData(0, randomCoord());
    updateTrackerData(1, randomCoord());
    updateNode(0, randomCoord(), '20th Century');

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

function randomCoord(num) {
    if (!num || num < 2)
        return [Math.random() * width, Math.random() * height];
    let array = [];
    while (num--)
        array.push(randomCoord(1));
    return array;
}

function circlePath(pos, r1, r2, a) {
    if (!r2)
        r2 = r1;
    if (!a)
        a = 0;
    return [['M', pos[0] + r1 * 0.65, pos[1] - r2 * 0.65],
            ['a', r1, r2, a, 1, 0, 0.001, 0.001],
            ['z']];
}

function updateTrackerData(id, pos) {
    if (!trackerData[id]) {
        trackerData[id] = canvas.path([['M', pos[0], pos[1]]])
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

function updateNode(id, pos, label) {
    if (!nodes.id) {
        nodes.id = canvas.path([['M', pos[0], pos[1]]]);
        nodes.id.label = canvas.text(pos[0], pos[1], label);
    }
    nodes.id.animate({'path': circlePath(pos, 50),
                      'fill': 'white'});
    nodes.id.label.animate({'x': pos[0],
                            'y': pos[1],
                            'stroke': 'black',
                            'font-size': 16});
}


