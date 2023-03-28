RegisterNetEvent('saveImg')
AddEventHandler('saveImg', function(saveDir, id)

	--TriggerEvent("test1")
	
	local src = source -- get the client that triggered the event
	--Citizen.Wait(1)
	--print("Player: "GetPlayers()[1])
	exports['screenshot-basic']:requestClientScreenshot(GetPlayers()[1], {
		fileName = saveDir.."images\\"..id..".jpg"
	}, function(err, data)
		print(saveDir.."images\\"..id..".jpg")

		--TriggerClientEvent("generateMetadata", src)
		TriggerClientEvent("saveMetadata", src)

	end)
	
end)


--[[RegisterNetEvent('saveMetadata')
AddEventHandler('saveMetadata', function(saveDir, id, metadata)
	
	local metadataFile =saveDir.."annotations\\"..id..".txt"
	
	--SaveResourceFile(GetCurrentResourceName(),metadataFile,"metadata")
	local file,err = io.open(metadataFile,'w')
    if file then
		print(metadataFile.. " Saved")
        file:write(tostring(metadata))
        file:close()
    else
        print("error:", err) 
    end
	
end)--]]
