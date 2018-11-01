"use strict"

window.onload = init;

var id = 1;
var canvas;
var width;
var height;
var cx;
var cy;

function init() {
    console.log('init!');

    canvas = Raphael($('#svgDiv')[0], '100%', '100%');
    width = ($('#svgDiv')[0]).offsetWidth;
    height = ($('#svgDiv')[0]).offsetHeight;
    console.log('window dimensions: '+width+'x'+height);
    cx = width * 0.5;
    cy = height * 0.5;

    function block(pos, size, label, fillopacity) {
        let x = pos.x - size.x * 0.5;
        let y = pos.y - size.y * 0.5;
        canvas.rect(x, y, size.x, size.y, 10)
              .attr({"stroke": "white",
                     "fill": "white",
                     "fill-opacity": fillopacity});
        if (label) {
            canvas.text(pos.x, pos.y, label)
                  .attr({"font-size": 36});
        }
    }

    function edge(start, stop) {
        canvas.path([['M', start.x, start.y],
                     ['L', stop.x, stop.y]])
        .attr({'stroke': 'white',
              'stroke-width': '2px',
              'arrow-end': 'block-wide-long'
              });
    }


    block({x:350, y:50}, {x:200, y:60}, "kinect");
    edge({x:350, y:80}, {x:350, y:115});
    block({x:350, y:150}, {x:200, y:60}, "tracker");
    edge({x:350, y:180}, {x:350, y:215});
    block({x:350, y:250}, {x:200, y:60}, "manager");

    edge({x:350, y:280}, {x:190, y:405});
    block({x:170, y:450}, {x:340, y:80}, null, 0);
    block({x:90, y:450}, {x:150, y:50}, "globe");
    block({x:250, y:450}, {x:150, y:50}, "wall");

    edge({x:350, y:280}, {x:510, y:405});
    block({x:530, y:450}, {x:340, y:80}, null, 0);
    block({x:450, y:450}, {x:150, y:50}, "floor");
    block({x:610, y:450}, {x:150, y:50}, "arch");
}
