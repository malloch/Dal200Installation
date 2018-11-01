"use strict"

window.onload = init;

var srcCodes = ["./CodeView.js",
                "../../Content/js/displayJs.js",
                "../Manager/Dal200Instalation/App.xaml.cs"];
var srcCodeIdx = 1;

function init() {
    console.log('CodeView init!');
    $('#code').empty()
              .load(srcCodes[srcCodeIdx]);

//    setInterval(function() {
//        console.log("flippin' code");
//        ++srcCodeIdx;
//        if (srcCodeIdx >= srcCodes.length)
//            srcCodeIdx = 0;
//        $('#code').empty()
//                  .load(srcCodes[srcCodeIdx]);
//    }, 5000);
}
