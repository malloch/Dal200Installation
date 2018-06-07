"use strict"

window.onload = init;

var id = 1;
var canvas;
var width;
var height;
var cx;
var cy;
var socket = null;

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
    socket = new WebSocket('ws://localhost/Dal200');
    socket.onopen = function(event) {
        console.log("Connection established");
        socket.send("a message from Joe");
    }
    socket.onmessage = function(event) {
        console.log("message received:", event);
    }


}


