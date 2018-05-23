stepHeight = 178;
stepDepth = 310;
stepWidth = 1940;
landingDepth = 1543;
stepsPerFlight = 9;

module step() {
    cube([stepWidth, stepDepth, stepHeight]);
}

module landing() {
    cube([stepWidth, landingDepth, stepHeight]);
}

module railing() {
    union() {
        color(c=[0.4, 0.4, 0.4]) {
            translate([-20, 0, 914])
                rotate([-60, 0, 0])
                    cylinder(h = 2860, r=20, center=false);
            translate([-20, stepDepth * 8, stepHeight * 8 + 914])
                rotate([-90, 0, 0])
                    cylinder(h = landingDepth - stepDepth, r= 20, center = false);
            translate([-20, stepDepth * 7 + landingDepth, stepHeight * 8 + 914])
                rotate([-60, 0, 0])
                    cylinder(h = 3217, r=20, center=false);
            translate([-20, stepDepth * 16 + landingDepth, stepHeight * 17 + 914])
                rotate([-90, 0, 0])
                    cylinder(h = landingDepth, r= 20, center = false);
        }
    }
}

module stairs() {
    union() {
        for (i = [0:stepsPerFlight-2]) {
            translate([0, i * stepDepth, i * stepHeight]) {
                step();
            }
        }
        // landing
        translate([0, 8 * stepDepth, 8*178]) {
            landing();
        }
        
        translate([0, 8 * stepDepth + 1543, 9 * stepHeight]) {
            for (i = [0:stepsPerFlight-2]) {
                translate([0, i * stepDepth, i * stepHeight]) {
                    step();
                }
            }
            // top
            translate([0, 8 * stepDepth, 8 * stepHeight]) {
                landing();
            }
        }
    }
    railing();
    translate([stepWidth+20, 0, 0])
        railing();        
}

stairs();
translate([-stepWidth-310, 0, 0]) {
    stairs();
    translate([-stepWidth-310, 0, 0])
        stairs();
}

module person(height) {
    color([1, 0.5, 0.5]) {
        union() {
            // torso
            translate([0, 0, height * 0.45])
                cylinder(h=height * 0.55, r=height/50, center=false);
            // head
            translate([0, 0, height * 15/16])
                sphere(r = height / 16);
            // leg1
            translate([100, 0, 0])
                cylinder(h=height * 0.45, r=height/50, center=false);
            // leg2
            translate([-100, 0, 0])
                cylinder(h=height * 0.45, r=height/50, center=false);
            // hips
            translate([-100, 0, height * 0.45])
                rotate([0, 90, 0])
                    cylinder(h=200, r=height/50, center=false);
            // shoulders
            translate([-200, 0, height * 0.8])
                rotate([0, 90, 0])
                    cylinder(h=400, r=height/50, center=false);
            // arm1
            translate([-200, 0, height * 0.48])
                cylinder(h=600, r=height/50, center=false);
            // arm2
            translate([200, 0, height * 0.48])
                cylinder(h=600, r=height/50, center=false);
        }
    }
}

translate([400, stepDepth * 9, stepHeight * 9])
    person(1830);

translate([700, stepDepth * 14.5, stepHeight * 11])
    person(1830);

translate([700, stepDepth * 22, stepHeight * 18])
    person(1830);

// crossbraces
module crossbrace(num) {
    for (i = [0:num-1]) {
        translate([0, 105, 60+i*360]) {
            rotate([45, 0, 0]) {
                cylinder(h = 150, r = 10, center = false);
            }
        }
        translate([0, 0, 240+i*360]) {
            rotate([-45, 0, 0]) {
                cylinder(h = 150, r = 10, center = false);
            }
        }
    }
}

// small triangular truss
module smallTruss() {
    color(c = [0.9, 0.9, 0.9]) {
        r = 30;
        union() {
            translate([-r, r, 0]) {
                cylinder(h = 1190, r = 30, center = false);
                translate([0, 150, 0]) {
                    cylinder(h = 1190, r = 30, center = false);
                }
                translate([-130, 75, 0]) {
                    cylinder(h = 1190, r = 30, center = false);
                }
            }
            translate([-r, r*1.8, 0]) {
                crossbrace(3);
            }
            translate([-140, 95, 0]) {
                rotate([0, 0, -120]) {
                    crossbrace(3);
                }
            }
            translate([-r*1.8, 165, 0])
                rotate([0, 0, 120])
                    crossbrace(3);
        }
    }
}

// medium triangular truss
module mediumTruss() {
    r = 30;
    color(c = [0.9, 0.9, 0.9]) {
        union() {
            translate([-r, r, 0]) {
                cylinder(h = 2210, r = 30, center = false);
                translate([0, 150, 0]) {
                    cylinder(h = 2210, r = 30, center = false);
                }
                translate([-130, 75, 0]) {
                    cylinder(h = 2210, r = 30, center = false);
                }
            }
            translate([-r, r*1.8, 0]) {
                crossbrace(6);
            }
            translate([-140, 95, 0]) {
                rotate([0, 0, -120]) {
                    crossbrace(6);
                }
            }
            translate([-r*1.8, 165, 0])
                rotate([0, 0, 120])
                    crossbrace(6);
        }
    }
}

// large triangular truss
module largeTruss() {
    r = 30;
    color(c = [0.9, 0.9, 0.9]) {
        union() {
            translate([-r, r, 0]) {
                cylinder(h = 2600, r = 30, center = false);
                translate([0, 150, 0]) {
                    cylinder(h = 2600, r = 30, center = false);
                }
                translate([-130, 75, 0]) {
                    cylinder(h = 2600, r = 30, center = false);
                }
            }
            translate([-r, r*1.8, 0]) {
                crossbrace(7);
            }
            translate([-140, 95, 0]) {
                rotate([0, 0, -120]) {
                    crossbrace(7);
                }
            }
            translate([-r*1.8, 165, 0])
                rotate([0, 0, 120])
                    crossbrace(7);
        }
    }
}

module smallMediumTruss() {
    smallTruss();
    translate([0, 0, 1190]) {
        mediumTruss();
    }
}

module smallSmallMediumTruss() {
    smallTruss();
    translate([0, 0, 1190]) {
        smallTruss();
        translate([0, 0, 1190])
            mediumTruss();
    }
}

module smallLargeTruss() {
    smallTruss();
    translate([0, 0, 1190]) {
        largeTruss();
    }
}

module mediumLargeTruss() {
    mediumTruss();
    translate([0, 0, 2210]) {
        largeTruss();
    }
}

module rod() {
    color(c = [0.9, 0.9, 0.9])
    cylinder(h = 2235, r = 30, center = false);
}

translate([-400, -400, 0]) {
    smallTruss();
    translate([-400, 0, 0]) {
        mediumTruss();
        translate([-400, 0, 0]) {
            largeTruss();
        }
    }
}

union() {
    stepNum = 5;
    translate([-310, stepNum*stepDepth-210, stepNum*stepHeight])
        smallSmallMediumTruss();
    translate([stepWidth, stepNum*stepDepth-210, stepNum*stepHeight])
        smallSmallMediumTruss();
    
    translate([-310, stepNum*stepDepth+2180, 9*stepHeight])
        smallLargeTruss();
    translate([stepWidth, stepNum*stepDepth+2180, 9*stepHeight])
        smallLargeTruss();
    
    translate([-30, stepNum*stepDepth-30, 5280]) {
        translate([-310, 2240, -400])
            rotate([90, 0, 0])
                rod();
        translate([-310 + 1100, 2240, 0])
            rotate([90, 0, 0])
                rod();
        translate([-310, 2240, 0])
            rotate([0, 90, 0])
                rod();
        translate([-310, 0, 0])
            rotate([0, 90, 0])
                rod();
    }
}