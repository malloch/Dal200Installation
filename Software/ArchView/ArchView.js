"use strict"

window.onload = init;

var id = 1;
var canvas;
var width;
var height;
var cx;
var cy;

var hue = 0;

var title1 = null;
var title2 = null;
var counter = 0;
var titleMode = 0;

var transition_interval = 10000;

function init() {
    console.log('init!');

    canvas = Raphael($('#svgDiv')[0], '100%', '100%');
    width = ($('#svgDiv')[0]).offsetWidth;
    height = ($('#svgDiv')[0]).offsetHeight;
    console.log('window dimensions: '+width+'x'+height);
    cx = width * 0.5;
    cy = height * 0.5;

    title1 = canvas.text(cx-130, 690, "Step Through History")
                   .attr({'font-size': 90,
                          'text-anchor': 'left',
                          'fill': 'white'});

    title2 = canvas.text(cx-130, 690, "Women in Computer Science")
                   .attr({'font-size': 60,
                          'text-anchor': 'left',
                          'fill': 'white',
                          'opacity': 0});

    setInterval(function() {
        textTransition();
    }, transition_interval);
}

function textTransition() {
    let elements = [title1, title2];
    let count = titleMode;
    while (count) {
        elements.push(elements.shift());
        --count;
    }
    elements[0].animate({'opacity': 0}, 1000, 'linear', function() {
        elements[1].toFront().animate({'opacity': 1}, 1000, 'linear');
    });
    titleMode++;
    if (titleMode > 1)
        titleMode = 0;
}
