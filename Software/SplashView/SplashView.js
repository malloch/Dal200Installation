"use strict"

window.onload = init;

var id = 1;
var canvas;
var width;
var height;
var cx;
var cy;

var footPathStart = null;
var footPathMid = null;
var footPathStop = null;

var footPath = null;
var footPathLen = 0;
var footPathProgress = 0;

var footColor = 'white';
var footFlip = 0;

var step_interval = 500;

var title = null;
var dal = null;
var gem = null;
var counter = 0;
var titleMode = 0;

var leftfoot = "M 0,50 c 7.484346,0.0567 13.328318,-7.33259 12.808633,-14.55702 0.245227,-6.71975 -3.251126,-12.98839 -7.915845,-17.59405 -4.207316,-5.56785 -3.728861,-14.34287 1.855842,-18.83665 4.430392,-4.37991 9.830818,-9.20799 9.636867,-16.00877 0.27532,-5.9523 -4.479349,-12.08656 -10.715927,-11.98528 -8.4087754,-0.76885 -17.2010574,1.76353 -23.8311884,6.99637 -5.8800276,4.97723 -8.3957906,13.06428 -7.1821866,20.57683 0.949631,10.04761 7.2961036,18.46598 9.3871386,28.18227 1.153372,5.10502 0.233333,10.64282 2.45671,15.47627 2.38427696,4.94707 7.820631,8.88976 13.4999564,7.75003 z m -25.06796,-74.4788 c 4.9261246,-1.55609 1.306667,-11.1816 -3.317027,-8.14194 -2.763132,2.4846 -0.696819,8.62908 3.317027,8.14194 z m 6.1715496,-6.45616 c 4.533662,-1.46137 1.972037,-9.71327 -2.4638456,-8.65601 -3.99585,1.74636 -1.800803,9.1704 2.4638456,8.65601 z m 32.6990914,-2.75151 c 6.740788,-2.81029 8.240908,-12.03977 5.051243,-18.00524 -3.446802,-4.57521 -10.614454,-1.14941 -11.255145,3.94226 -1.789435,4.63081 -1.155078,11.17145 3.499359,13.8357 0.854593,0.33011 1.806517,0.42821 2.704543,0.22728 z m -25.5667614,-1.7857 c 5.686873,-0.98764 5.307824,-11.07129 -0.559262,-11.37842 -6.185994,0.35674 -5.734774,11.76539 0.559262,11.37842 z m 11.72649,-0.45023 c 6.175115,-2.2797 7.188629,-12.12867 1.560781,-15.57645 -4.929094,-1.22868 -8.29656,4.45148 -7.828502,8.77768 0.223245,3.21568 2.38357,7.60543 6.267721,6.79877 z";

var rightfoot = "M 0,50 c -7.484346,0.0567 -13.328318,-7.33259 -12.808633,-14.55702 -0.245227,-6.71975 3.251126,-12.98839 7.915845,-17.59405 4.207316,-5.56785 3.728861,-14.34287 -1.855842,-18.83665 -4.430392,-4.37991 -9.830818,-9.20799 -9.636867,-16.00877 -0.27532,-5.9523 4.479349,-12.08656 10.715927,-11.98528 8.408775,-0.76885 17.201057,1.76353 23.831188,6.99637 5.880028,4.97723 8.395791,13.06428 7.182187,20.57683 -0.949631,10.04761 -7.296104,18.46598 -9.387139,28.18227 -1.153372,5.10502 -0.233333,10.64282 -2.45671,15.47627 -2.384277,4.94707 -7.820631,8.88976 -13.499956,7.75003 z m 25.06796,-74.4788 c -4.926125,-1.55609 -1.306667,-11.1816 3.317027,-8.14194 2.763132,2.4846 0.696819,8.62908 -3.317027,8.14194 z m -6.17155,-6.45616 c -4.533662,-1.46137 -1.972037,-9.71327 2.463846,-8.65601 3.99585,1.74636 1.800803,9.1704 -2.463846,8.65601 z m -32.699091,-2.75151 c -6.740788,-2.81029 -8.240908,-12.03977 -5.051243,-18.00524 3.446802,-4.57521 10.614454,-1.14941 11.255145,3.94226 1.789435,4.63081 1.155078,11.17145 -3.499359,13.8357 -0.854593,0.33011 -1.806517,0.42821 -2.704543,0.22728 z m 25.566761,-1.7857 c -5.686873,-0.98764 -5.307824,-11.07129 0.559262,-11.37842 6.185994,0.35674 5.734774,11.76539 -0.559262,11.37842 z m -11.72649,-0.45023 c -6.175115,-2.2797 -7.188629,-12.12867 -1.560781,-15.57645 4.929094,-1.22868 8.29656,4.45148 7.828502,8.77768 -0.223245,3.21568 -2.38357,7.60543 -6.267721,6.79877 z";

function init() {
    console.log('init!');

    canvas = Raphael($('#svgDiv')[0], '100%', '100%');
    width = ($('#svgDiv')[0]).offsetWidth;
    height = ($('#svgDiv')[0]).offsetHeight;
    console.log('window dimensions: '+width+'x'+height);
    cx = width * 0.5;
    cy = height * 0.5;

    setInterval(function() {
        walk();
        if (counter++ > 20) {
            textTransition();
            counter = 0;
        }
    }, step_interval);

    $('body').on('keydown.list', function(e) {
        switch (e.which) {
            case 32:
                /* space */
                walk();
                break;
        }
    })

    title = canvas.text(width*0.5, 300, "Step Through History:\nWomen in Computer Science")
                  .attr({'font-size': 80,
                         'text-anchor': 'left',
                         'fill': 'white'});

    dal = canvas.image("./logos/Dal_logo.png", width*0.2, height*0.4, width*0.6, height*0.2)
                .attr({'opacity': 0});

    gem = canvas.image("./logos/GEM_logo_white.png", width*0.3, height*0.3, width*0.4, height*0.3)
                .attr({'opacity': 0});

//    var instruction = canvas.text(width*0.5, 600, "Step below to activate:")
//                            .attr({'font-size': 40,
//                                   'text-anchor': 'left',
//                                   'fill': 'white'});
}

function textTransition() {
    let elements = [title, dal, gem];
    let count = titleMode;
    while (count) {
        elements.push(elements.shift());
        --count;
    }
    elements[0].animate({'opacity': 0}, 1000, 'linear');
    elements[1].animate({'opacity': 0}, 1000, 'linear', function() {
        elements[2].toFront().animate({'opacity': 1}, 1000, 'linear');
    });
    titleMode++;
    if (titleMode > 2)
        titleMode = 0;
}

function randomCoord(w=width, h=height) {
    return {'x': Math.random() * w, 'y': Math.random() * h};
}

function rollDice(numsides) {
    return Math.floor(Math.random() * numsides);
}

function randomBorderCoord(exclude) {
    let x, y;
    let border = rollDice(4);
    while (border == exclude)
        border = rollDice(4);
    console.log('border', exclude);
    switch (border) {
        case 0:
            // left
            x = 0;
            y = Math.random() * height;
            break;
        case 1:
            // right
            x = width;
            y = Math.random() * height;
            break;
        case 2:
            // top
            x = Math.random() * width;
            y = 0;
            break;
        default:
            // bottom
            x = Math.random() * width;
            y = height;
            break;
    }
    return {'x': x, 'y': y, 'b': border};
}

function walk() {
    if (!footPath)
        footPath = canvas.path()
                         .attr({'stroke-opacity': 0, 'stroke': 'white'});

    if (!footPathLen || footPathProgress > footPathLen) {
        // need a new path
        footColor = Raphael.getColor();
        footPathStart = randomBorderCoord();
        footPathMid = randomCoord();
        footPathStop = randomBorderCoord(footPathStart.b);

        footPath.attr({'path': [['M', footPathStart.x, footPathStart.y],
                                ['Q', footPathMid.x, footPathMid.y,
                                 footPathStop.x, footPathStop.y]]});
        footPathLen = footPath.getTotalLength();
        footPathProgress = 0;
    }
//    footColor = Raphael.getColor();

    let pos = footPath.getPointAtLength(footPathProgress);

    // adjust angles
    let angle;
    if (footPathProgress < footPathLen * 0.4) {
        // if progress < halfway, aim towards midpoint
        angle = Raphael.angle(pos.x, pos.y, footPathMid.x, footPathMid.y);
    }
    else {
        // else aim towards stoppoint;
        angle = Raphael.angle(pos.x, pos.y, footPathStop.x, footPathStop.y);
    }
    while (pos.alpha > (angle + 90)) {
        pos.alpha -= 180;
    }
    while (pos.alpha < (angle - 90)) {
        pos.alpha += 180;
    }
    pos.alpha += 270;
    pos.alpha %= 360;
    if (pos.alpha < 360)
        pos.alpha += 360;

    let foot = canvas.path(footFlip ? rightfoot : leftfoot)
                     .attr({'fill': footColor, 'stroke-opacity': 0})
                     .transform("t"+pos.x+","+pos.y+"r"+(pos.alpha));
    foot.animate({'fill-opacity': 0}, 20000, 'linear', function() {
        this.remove();
    });

    footPathProgress += 150;
    footFlip = footFlip ? 0 : 1;
}
