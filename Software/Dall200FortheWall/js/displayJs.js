
var dataset=[];

var pagePoints=[];

var widthOfContentDiv="300px";
var heightOfContentDiv="500px";


var DataCollectedFromController = [];

var positionXY = [];


//left and top positions on the wall
var wallSections = [["300px","500px"],["700px","400px"],["200px","300px"],["900px","500px"]];

var intIteration =0;

var idText = 0; 

var TextToShow="";

   //sum of values in a row
    d3.csv("datafordisplay.csv", function (ignData) {
        dataset = ignData;

    })

//get data from textbox and display appropriate text
    function shower(){

        d3.select("#firstDiv").remove();

        idText = d3.select("#txtId").property('value');
        console.log(dataset[idText].wordings);
        console.log(dataset);



//the div holding th e image and text
        var divWithContent= d3.select("body").append("div")
            .attr("id","firstDiv")
            .style("width",widthOfContentDiv)
            .style("height",heightOfContentDiv)
            .style("background-color", "red");
            divWithContent.append("img")
                .attr("src", "images/"+dataset[idText].media)
                .style("width",widthOfContentDiv);
            divWithContent.append("p")
                .style("color", "white").text(dataset[idText].wordings);

            divWithContent.style("position","absolute")
            .transition().duration(500).style("left","200px").style("top","300px");
           
     }


function moveDiv(){

    var ddlSelectedPosition = d3.select("#ddlPosition").property('value');

    d3.select("#firstDiv").transition().duration(500)
        .style("left", wallSections[intIteration][0]).style("top",wallSections[intIteration][1]);
    
        console.log(wallSections[0][0]);

}



function WebSocketTest()
         {
            if ("WebSocket" in window)
            {
               alert("WebSocket is supported by your Browser!");
               
               // Let us open a web socket
               var ws = new WebSocket("ws://129.173.66.128/Dal200");
				
               ws.onopen = function()
               {
                  // Web Socket is connected, send data using send()
                  ws.send("Message to send");
                  alert("Message is sent...");
               };
				
               ws.onmessage = function (evt) 
               { 
                  DataCollectedFromController = evt.data;
                  var received_msg = evt.data;
                  console.log(DataCollectedFromController);

                  

                  //movement of div
                  if (intIteration == 3){

                    intIteration=0;
                  }
                  else{
                    intIteration+=1;
                  }

                  moveDiv();
                  console.log("recieved: " + intIteration);
                  //alert("Message is received...");
               };
				
               ws.onclose = function()
               { 
                  // websocket is closed.
                  alert("Connection is closed..."); 
               };
					
               window.onbeforeunload = function(event) {
                  socket.close();
               };
            }
            
            else
            {
               // The browser doesn't support WebSocket
               alert("WebSocket NOT supported by your Browser!");
            }
         }

         WebSocketTest();










     function moveDivJQ(){
        
        $(document).ready(function(){
            $("#showButton").click(function(){
                $("#testo").animate({left: '250px'});
            }); 
        });
       
        
     }



/*
 $.ajax({
   type: "GET",  
   url: "datafordisplay.csv",
   dataType: "text",       
   success: function(response)  
   {
    dataset = $.csv.toArrays(response);

    

   }   
 });


 function shower(){
    idText = d3.select("#txtId").property('value');
    console.log(dataset[idText][1]);
    console.log(dataset);
 }
*/