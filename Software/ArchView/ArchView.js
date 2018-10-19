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
var dal = null;
var gem = null;
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

    // canvas.path().attr({'stroke': 'blue',
    //                     'path': [['M', 0, 0],
    //                              ['l', width, 0],
    //                              ['l', 0, height],
    //                              ['l', -width, 0],
    //                              ['z']]});

    title1 = canvas.text(cx-130, 690, "Step Through History")
                   .attr({'font-size': 90,
                          'text-anchor': 'left',
                          'fill': 'white'});

    title2 = canvas.text(cx-130, 690, "Women in Computer Science")
                   .attr({'font-size': 60,
                          'text-anchor': 'left',
                          'fill': 'white',
                          'opacity': 0});

    dal = canvas.image("./logos/Dal_logo.png", 250, 630, 467, 120)
                .attr({'opacity': 0});

    gem = canvas.image("./logos/GEM_logo_white.png", 160, 630, 300, 142)
                .attr({'opacity': 0});

//    var instruction = canvas.text(width*0.5, 600, "Step below to activate:")
//                            .attr({'font-size': 40,
//                                   'text-anchor': 'left',
//                                   'fill': 'white'});

    setInterval(function() {
        textTransition();
    }, transition_interval);
}

function textTransition() {
    let elements = [title1, title2];//, dal, gem];
    let count = titleMode;
    while (count) {
        elements.push(elements.shift());
        --count;
    }
    // elements[0].animate({'opacity': 0}, 1000, 'linear');
    // elements[1].animate({'opacity': 0}, 1000, 'linear');
    elements[0].animate({'opacity': 0}, 1000, 'linear', function() {
        elements[1].toFront().animate({'opacity': 1}, 1000, 'linear');
    });
    titleMode++;
    if (titleMode > 1)
        titleMode = 0;
}
