using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using MenuAPI;
//using Newtonsoft.Json;
using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.UI.Screen;
using static CitizenFX.Core.Native.API;


using System.IO;


namespace DatasetGenerator.Client
{
    public class ClientMain : BaseScript
    {
        
        private bool debugMode = false;
        private bool showBoxMode = false;
        private bool collectMode = false;
        private int currentID = 0;
        public static int getInfoKey = 170; //f3
        public static int debugKey = 166; //f5
        public static int showBoxKey = 167; //f6
        public static int collectKey = 288; //f1
        public static int carOnlyCollectKey = 168; //f8
        internal static float entityRange = 15000f;//15000f;
        public float targetSpeed = 30f;
        private int picsFromLocation = 1000;
        private int ticksBetweenPics = 60;

        
        string saveDir = @"E:\Dissertation\dataset\";

        private List<Vehicle> vehicles = new List<Vehicle>();
        private Vector3 playerPos;
        private string filePath;
        private List<string> metadataList;
        private int tickCounter = 0;
        private string metadataString = "";
        private bool saveMetadata = false;
        private bool vehiclesFrozen = false;
        private DateTime start;
        private List<Vector3> vehicleVelocities;
        private Dictionary<int, Vector3> carVelocityDict ;
        private bool canStart = false;
        private Location location1;
        private Location location2;
        private bool vehiclesOnScreen=false;
        //private List<Location> locations;
        private Location[] locations;
        private int currentLocationIndex = 0;
        /*private int[] times = {0,6,12,18};
        string[] weathers = {"CLEAR", "RAIN", "FOGGY", "SNOW"};*/
        


        #region Drawing text on screen
        public static void DrawTextOnScreen(string text, float xPosition, float yPosition, float size, CitizenFX.Core.UI.Alignment justification, int font, bool disableTextOutline)
        {
            
            SetTextFont(font);
            SetTextScale(1.0f, size);
            if (justification == CitizenFX.Core.UI.Alignment.Right)
            {
                SetTextWrap(0f, xPosition);
            }
            SetTextJustification((int)justification);
            if (!disableTextOutline) { SetTextOutline(); }
            BeginTextCommandDisplayText("STRING");
            AddTextComponentSubstringPlayerName(text);
            EndTextCommandDisplayText(xPosition, yPosition);
            
        }
        
        /// <summary>
        /// Draw text on the screen at the provided x and y locations.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="xPosition">The x position for the text draw origin.</param>
        /// <param name="yPosition">The y position for the text draw origin.</param>
        /// <param name="size">The size of the text.</param>
        /// <param name="justification">Align the text. 0: center, 1: left, 2: right</param>
        /// <param name="font">Specify the font to use (0-8).</param>
        public static void DrawTextOnScreen(string text, float xPosition, float yPosition, float size, CitizenFX.Core.UI.Alignment justification, int font) =>
            DrawTextOnScreen(text, xPosition, yPosition, size, justification, font, false);

        #endregion

        #region Bounding Boxes
            
        /// <summary>
        /// Gets the bounding box of the entity model in world coordinates, used by <see cref="DrawEntityBoundingBox(Entity, int, int, int, int)"/>.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        internal static Vector3[] GetEntityBoundingBox(int entity)
        {
            Vector3 min = Vector3.Zero;
            Vector3 max = Vector3.Zero;

            GetModelDimensions((uint)GetEntityModel(entity), ref min, ref max);
            const float pad = 0f;
            //const float pad = 0.001f;
            var retval = new Vector3[8]
            {
                // Bottom
                GetOffsetFromEntityInWorldCoords(entity, min.X - pad, min.Y - pad, min.Z - pad),
                GetOffsetFromEntityInWorldCoords(entity, max.X + pad, min.Y - pad, min.Z - pad),
                GetOffsetFromEntityInWorldCoords(entity, max.X + pad, max.Y + pad, min.Z - pad),
                GetOffsetFromEntityInWorldCoords(entity, min.X - pad, max.Y + pad, min.Z - pad),

                // Top
                GetOffsetFromEntityInWorldCoords(entity, min.X - pad, min.Y - pad, max.Z + pad),
                GetOffsetFromEntityInWorldCoords(entity, max.X + pad, min.Y - pad, max.Z + pad),
                GetOffsetFromEntityInWorldCoords(entity, max.X + pad, max.Y + pad, max.Z + pad),
                GetOffsetFromEntityInWorldCoords(entity, min.X - pad, max.Y + pad, max.Z + pad)
            };
            return retval;
        }

         /// <summary>
        /// Draws the edge poly faces and the edge lines for the specific box coordinates using the specified rgba color.
        /// </summary>
        /// <param name="box"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        private static void DrawBoundingBox(Vector3[] box, int r, int g, int b, int a)
        {
            var polyMatrix = GetBoundingBoxPolyMatrix(box);
            var edgeMatrix = GetBoundingBoxEdgeMatrix(box);

            DrawPolyMatrix(polyMatrix, r, g, b, a);
            DrawEdgeMatrix(edgeMatrix, 255, 255, 255, 255);
        }

        /// <summary>
        /// Gets the coordinates for all poly box faces.
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        private static Vector3[][] GetBoundingBoxPolyMatrix(Vector3[] box)
        {
            return new Vector3[12][]
            {
                new Vector3[3] { box[2], box[1], box[0] },
                new Vector3[3] { box[3], box[2], box[0] },

                new Vector3[3] { box[4], box[5], box[6] },
                new Vector3[3] { box[4], box[6], box[7] },

                new Vector3[3] { box[2], box[3], box[6] },
                new Vector3[3] { box[7], box[6], box[3] },

                new Vector3[3] { box[0], box[1], box[4] },
                new Vector3[3] { box[5], box[4], box[1] },

                new Vector3[3] { box[1], box[2], box[5] },
                new Vector3[3] { box[2], box[6], box[5] },

                new Vector3[3] { box[4], box[7], box[3] },
                new Vector3[3] { box[4], box[3], box[0] }
            };
        }

        /// <summary>
        /// Gets the coordinates for all edge coordinates.
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        private static Vector3[][] GetBoundingBoxEdgeMatrix(Vector3[] box)
        {
            return new Vector3[12][]
            {
                new Vector3[2] { box[0], box[1] },
                new Vector3[2] { box[1], box[2] },
                new Vector3[2] { box[2], box[3] },
                new Vector3[2] { box[3], box[0] },

                new Vector3[2] { box[4], box[5] },
                new Vector3[2] { box[5], box[6] },
                new Vector3[2] { box[6], box[7] },
                new Vector3[2] { box[7], box[4] },

                new Vector3[2] { box[0], box[4] },
                new Vector3[2] { box[1], box[5] },
                new Vector3[2] { box[2], box[6] },
                new Vector3[2] { box[3], box[7] }
            };
        }

        public static void DrawEntityBoundingBox(Entity ent, int r, int g, int b, int a)
        {
            // list of length 8 for all the corners
            var box = GetEntityBoundingBox(ent.Handle);
            DrawBoundingBox(box, r, g, b, a);
        }

         /// <summary>
        /// Draws the poly matrix faces.
        /// </summary>
        /// <param name="polyCollection"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        private static void DrawPolyMatrix(Vector3[][] polyCollection, int r, int g, int b, int a)
        {
            foreach (var poly in polyCollection)
            {
                float x1 = poly[0].X;
                float y1 = poly[0].Y;
                float z1 = poly[0].Z;

                float x2 = poly[1].X;
                float y2 = poly[1].Y;
                float z2 = poly[1].Z;

                float x3 = poly[2].X;
                float y3 = poly[2].Y;
                float z3 = poly[2].Z;
                DrawPoly(x1, y1, z1, x2, y2, z2, x3, y3, z3, r, g, b, a);
            }
        }

        /// <summary>
        /// Draws the edge lines for the model dimensions.
        /// </summary>
        /// <param name="linesCollection"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        private static void DrawEdgeMatrix(Vector3[][] linesCollection, int r, int g, int b, int a)
        {
            foreach (var line in linesCollection)
            {
                float x1 = line[0].X;
                float y1 = line[0].Y;
                float z1 = line[0].Z;

                float x2 = line[1].X;
                float y2 = line[1].Y;
                float z2 = line[1].Z;

                DrawLine(x1, y1, z1, x2, y2, z2, r, g, b, a);
            }
        }


        #endregion

        public ClientMain()
        {
            
            Debug.WriteLine("DatasetGenerator.Client activated");
                                                                     
            locations = new Location[]{
                // Highways
                new Location(new Vector3(-1789.430f, -650.141f, 20f), 5.567f, new Vector3(0.000f, 0.000f, 5.567f),-17.59277f, 30f),
                new Location(new Vector3(-2681.45f, 2616.035f, 19.39864f), -6.830189E-06f, new Vector3(0f, 0f, -47.18701f), -2.454675f, 30f),
                new Location(new Vector3(-1162.208f, 5250.1f, 75.11191f), 6.830189E-06f, new Vector3(0f, 0f, 71.09592f), -31.23303f, 30f),
                new Location(new Vector3(2609.852f, 5173.946f, 78.83096f), -1.366038E-05f, new Vector3(0f, 0f, -167.2793f), -28.05221f, 30f),
                new Location(new Vector3(2481.143f, 2967.247f, 64.61263f), 0f, new Vector3(0f, 0f, 127.8405f), -12.3284f, 30f),
                // Urban
                new Location(new Vector3(1769.527f, 3540.025f, 36.594f), 339.961f, new Vector3(0.000f, 0.000f, -20.039f),-1.670777f, 10f),
                new Location(new Vector3(-640.952f, -1531.846f, 22.94387f), 6.830189E-06f, new Vector3(0f, 0f, 103.7186f), -10.77995f, 10f), //slow
                new Location(new Vector3(-294.5515f, -1821.041f, 49.27009f), -6.830189E-06f, new Vector3(0f, 0f, -104.069f), -20.70007f, 10f),
                new Location(new Vector3(81.70882f, -1906.543f, 25.84157f), 3.415094E-06f, new Vector3(0f, 0f, 42.32573f), -6.533714f, 10f),
                new Location(new Vector3(910.7935f, -2470.437f, 28.65929f), 0f, new Vector3(0f, 0f, 41.96225f), 1.877675f, 10f),
                new Location(new Vector3(1389.423f, -1631.4f, 63.67603f), 6.830189E-06f, new Vector3(0f, 0f, 75.31506f), -13.27346f, 10f),
                new Location(new Vector3(1125.738f, -513.2305f, 72.53145f), -6.830189E-06f, new Vector3(0f, 0f, -98.63248f), -6.08167f, 10f),
                new Location(new Vector3(1065.598f, -534.1071f, 64.73195f), 6.830189E-06f, new Vector3(0f, 0f, 97.7282f), -3.598453f, 10f),
                new Location(new Vector3(863.1425f, -511.2666f, 58.13159f), -1.366038E-05f, new Vector3(0f, 0f, -139.1936f), 2.177621f, 10f),
                new Location(new Vector3(548.1376f, -189.2076f, 79.43042f), 6.830189E-06f, new Vector3(0f, 0f, 58.4398f), -21.22769f, 10f),
                new Location(new Vector3(367.6646f, 116.7666f, 109.1424f), 1.366038E-05f, new Vector3(0f, 0f, 119.7441f), -19.6664f, 10f),
                new Location(new Vector3(199.72f, 211.8542f, 109.1424f), 6.830189E-06f, new Vector3(0f, 0f, 103.8686f), -4.056938f, 10f),
                new Location(new Vector3(-917.3553f, 222.5634f, 70.46163f), -4.268868E-07f, new Vector3(0f, 0f, -5.603947f), -1.524998f, 10f),
                new Location(new Vector3(-1812.643f, 741.5195f, 147.9204f), 6.830189E-06f, new Vector3(0f, 0f, 93.5533f), -13.79819f, 10f),
                new Location(new Vector3(-1561.443f, 2141.757f, 63.60139f), -6.830189E-06f, new Vector3(0f, 0f, -95.46081f), -4.624403f, 10f)
            };

            


            EventHandlers["updateMetadata"] += new Action<bool> (UpdateMetadata);
            EventHandlers["saveMetadata"] += new Action (SaveMetadata);
            Tick += OnTick;
        }


        private void FreezeVehicles(bool freezeMode){

            Vehicle[] allNearbyCars = World.GetAllVehicles();

            if(freezeMode){

                vehicleVelocities = new List<Vector3>{};
                carVelocityDict = new Dictionary<int, Vector3>();

                foreach (Vehicle v in allNearbyCars){
                    int currentVeh = v.Handle;
                    Vector3 currentVel = GetEntityVelocity(currentVeh);
                    if(currentVel==new Vector3(0,0,0) || currentVel.Length()<targetSpeed/4){ //Target speed /6   targetSpeed/4 7.5f
                        DeleteEntity(ref currentVeh);
                    }else{
                        carVelocityDict.Add(currentVeh, currentVel);
                        FreezeEntityPosition(currentVeh, true);
                    }
                    
                 }
                 
                vehiclesFrozen=true;
                
            }else{
                foreach(KeyValuePair<int, Vector3> item in carVelocityDict){
                        int currentVehicle = item.Key;
                        Vector3 currentVelocity = item.Value;
                        FreezeEntityPosition(currentVehicle, false);
                        float currentSpeed = currentVelocity.Length();
                        
                        if(currentVelocity.Length()<targetSpeed){
                            float modifier = 1+(targetSpeed-currentVelocity.Length())/(targetSpeed*2); //*2
                            SetEntityVelocity(currentVehicle,currentVelocity.X*modifier,currentVelocity.Y*modifier,currentVelocity.Z); //1.35f
                        }else{
                            SetEntityVelocity(currentVehicle,currentVelocity.X,currentVelocity.Y,currentVelocity.Z);
                        }
                        
                }
                vehiclesFrozen=false;
            }
        }

        public void StopDataCollection(int playerEntity){
            collectMode=false; 
            canStart=false;
            //currentLocationIndex=0;

            FreezeEntityPosition(playerEntity, false);
            SetEntityInvincible(playerEntity, false);

            //Set's third person mode
            SetFollowPedCamViewMode(0);

            //Teleports player back to spawn location
            SetEntityCoords(playerEntity, -1257.721f, -1479.454f, 3.257412f, false, false, false, true);
        }

        public Task OnTick()
        {   
            //Updates the metadata without updating coordinate variables (for the onscreen bounding boxes)
            UpdateMetadata(false);

            int playerEntity = Game.PlayerPed.Handle; // set this as a global variable is probably a good idea
            
            if (collectMode){
                tickCounter++;
            }else{
                tickCounter=0;
            }
            
            #region  If get info key is pressed f3
                if (Game.IsControlJustPressed(0, (Control)getInfoKey)){
                    Vector3 pCoords = GetEntityCoords(playerEntity, true);
                    float pHeading =    GetGameplayCamRelativeHeading();//GetEntityHeading(playerEntity);//GetGameplayCamRelativeHeading();
                 
                    Vector3 pRotation = GetEntityRotation(playerEntity,2);
                    float pPitch = GetGameplayCamRelativePitch();
                    float carSpeedTarget = 10f;
                    //Debug.WriteLine($"PITCH:{GetGameplayCamRelativePitch()}");
                    Debug.WriteLine($" new Location(new Vector3({pCoords.X}f, {pCoords.Y}f, {pCoords.Z}f), {pHeading}f, new Vector3({pRotation.X}f, {pRotation.Y}f, {pRotation.Z}f), {pPitch}f, {carSpeedTarget}f),");
                    //new Location(new Vector3(1769.527f, 3540.025f, 36.594f), 339.961f, new Vector3(0.000f, 0.000f, -20.039f),-1.670777f, 30);
                    
                }
                #endregion

            #region Change debug mode if key is pressed f5
                if (Game.IsControlJustPressed(0, (Control)debugKey)){
                    
                    debugMode=!debugMode;
                    
                }
                #endregion

            #region Change show box mode if key is pressed f6
                if (Game.IsControlJustPressed(0, (Control)showBoxKey)){
                    
                    showBoxMode=!showBoxMode;
                    Debug.Write($"SHOW BOX MODE: {showBoxMode}");
                    Debug.WriteLine($"PITCH:{GetGameplayCamRelativePitch()}");
                    
                }
                #endregion

            #region Change collect mode if key is pressed f1

            if (Game.IsControlJustPressed(0, (Control)collectKey)){
                    
                    if(!collectMode){ // If it is being changed to collect mode
                        
                        currentLocationIndex=0;
                        
                        Location currentLocation = locations[currentLocationIndex]; // repeated code block :(
                        targetSpeed = currentLocation.GetSpeed();
                        currentLocation.SetLocation(playerEntity);
                        collectMode=true;
                        
                    }else{
                        StopDataCollection(playerEntity);
                    }
                }
                #endregion

            
            if (collectMode && tickCounter>=ticksBetweenPics){ 
                

                if(!canStart){
                    if (tickCounter<1500){ 
                        tickCounter+=1;
                    }else{
                        canStart= true;
                    }
                }else if(vehiclesOnScreen){ // if all parameters for taking a screenshot and saving metadata are true

                    //Resets timer for taking screenshots
                    tickCounter=0;

                    Location currentLocation = locations[currentLocationIndex];
                   
                    //Set Condition (time and weather)
                    currentLocation.SetCondition();
                    //Freezes all nearby vehicles
                    FreezeVehicles(true);
                    //Triggers the event to save the screenshot
                    TriggerServerEvent("saveImg", saveDir, currentID); //See ../Server/ServerSaveScreenshot.lua
                    //Updates the metadata and updates coordinate variables
                    UpdateMetadata(true);
                }
            
                if(!locations[currentLocationIndex].getShouldContinue()){ // if enough picsd have been taken from this location
                        
                    if(currentLocationIndex!=(locations.Length)-1){  // if there is another location to go 
                        collectMode= false;
                        canStart=false;
                        currentLocationIndex+=1;

                        Location currentLocation = locations[currentLocationIndex]; // repeated code block :(
                        targetSpeed = currentLocation.GetSpeed();
                        currentLocation.SetLocation(playerEntity);
                        collectMode=true;

                    }else{
                        StopDataCollection(playerEntity);
                    }    
                }
            }

        return Task.FromResult(0);
        
        }
        private void UpdateMetadata(bool save){

            playerPos = Game.PlayerPed.Position;
            vehicles = World.GetAllVehicles().Where(e => e.IsOnScreen && e.Position.DistanceToSquared(playerPos) < entityRange && HasEntityClearLosToEntity(PlayerPedId(), e.Handle, 17)).ToList(); 

            if(vehicles.Count>0){
                vehiclesOnScreen = true;
            }else{
                vehiclesOnScreen = false;
            }
            
            if(save){
                metadataList = new List<string>(){};
            }
            
            foreach (Vehicle v in vehicles)
            {
                
                if(debugMode){
                    DrawEntityBoundingBox(v, 250, 150, 0, 100);
                }

                List<int> pointCoordsX = new List<int>(){};
                List<int> pointCoordsY = new List<int>(){};

                //float xVal =0f;
                //float yVal=0f;

                int xScreen=0;
                int yScreen=0;

                var vehicleBoxes = GetEntityBoundingBox(v.Handle);

                //GetScreenCoordFromWorldCoord(v.Position.X, v.Position.Y, v.Position.Z, ref xVal,ref yVal);
                GetActiveScreenResolution(ref xScreen, ref yScreen);

                foreach(Vector3 vehicleBox in vehicleBoxes){

                    float xVal =0f;
                    float yVal=0f;

                    GetScreenCoordFromWorldCoord(vehicleBox.X, vehicleBox.Y, vehicleBox.Z, ref xVal,ref yVal);
                    
                    int currentXCoord = (int)(xVal*xScreen);
                    int currentYCoord = (int)(yVal*yScreen);  

                    if(debugMode){
                        SetDrawOrigin(vehicleBox.X, vehicleBox.Y, vehicleBox.Z, 0);
                        DrawTextOnScreen($"{currentXCoord},{currentYCoord}", 0f, 0f, 0.3f, Alignment.Center, 0);
                        ClearDrawOrigin();
                    }
                    pointCoordsX.Add(currentXCoord);
                    pointCoordsY.Add(currentYCoord);
                }

                
                int minX = pointCoordsX.Min();
                int minY = pointCoordsY.Min();

                int maxX = pointCoordsX.Max();
                int maxY = pointCoordsY.Max();

                
                if(debugMode){
                    SetDrawOrigin(v.Position.X, v.Position.Y, v.Position.Z+1.5f, 0);
                    DrawTextOnScreen($"{v.DisplayName}\n{v.ClassLocalizedName}", 0f, 0f, 0.3f, Alignment.Center, 0);
                    ClearDrawOrigin();
                }


                // Calculate the center point of the rectangle
                float centerX = (float)(minX + maxX) / 2.0f;
                float centerY = (float)(minY + maxY) / 2.0f;

                // Calculate the width and height of the rectangle
                float width = (float)(maxX - minX);
                float height = (float)(maxY - minY);

                // Convert the pixel coordinates to relative screen coordinates
                float relativeX = centerX / xScreen;
                float relativeY = centerY / yScreen;
                float relativeWidth = width / xScreen;
                float relativeHeight = height / yScreen;

                
                if (minX >= 0 && minY >= 0 && maxX < xScreen && maxY < yScreen){ //If the full bounding box is on the screen
                    if(showBoxMode){
                        DrawRect(relativeX, relativeY, relativeWidth, relativeHeight, 100, 255, 255, 150);
                    }
                    
                    if(save){
                        metadataList.Add($"{v.ClassDisplayName.Split('_').Last()} {relativeX} {relativeY} {relativeWidth} {relativeHeight}\n"); //TODO: Fix this issue with the number
                    }
                }  
            }
            if(save){
                //Debug.WriteLine("UPDATED METADATA");
            }
        }
        private void SaveMetadata(){

            FreezeVehicles(false);
            //Debug.Write("SAVING METADATA");
            var metadataString =String.Join("",metadataList);
            TriggerServerEvent("saveData", saveDir, currentID, metadataString);
            currentID++;
            
        }
        private void ChangeLocation(int locationIndex, int player){
            Location currentLocation = locations[locationIndex]; // repeated code block :(
            targetSpeed = currentLocation.GetSpeed();
            currentLocation.SetLocation(player);
            collectMode=true;
        }
    }

    class Location{

        Vector3 coords;
        float heading;
        Vector3 rotation;
        float pitch;
        float speed;
        int imageCounter = 0;
        int currentTime = 0;
        int currentWeather = 0;
        int targetImageNum;
        private int[] times = {0,6,12,18};
        string[] weathers = {"CLEAR", "RAIN", "FOGGY"};

        string[] vehicles = {
            "adder", "cheetah", "entityxf", "zentorno", "t20", //Super
            "seminole", "rocoto", "gresley", "baller", "baller2", //SUV
            "burrito", "rumpo", "pony", "speedo", "youga", //vans
            "rapidgt", "carbonizzare", "banshee", "massacro", "pariah", // sports
            "asterope", "intruder", "primo", "stanier", "schafter2", //Sedan
            "dominator", "gauntlet", "vigero", "stalion", "dominator3", //Muscle
            "kamacho", "rebel2", "sandking2", "mesa3", "rancherxl", //off-road
            "bati", "sanchez2", "hakuchou", "zombieb", "fcr", //Motorbikes
            "tribike3", "scorcher", "bmx", "fixter", "cruiser", //Cycles
            "oracle", "felon", "jackal", "sentinel2", "zion", //coupe
            "phantom", "benson", "mule", "biff", "stockade", //commercial 
            "issi3", "brioso", "rhapsody", "panto", "issi2", // compacts
        
        
        };
        private bool shouldContinue = true;

        public Location(Vector3 playerCoords, float cameraHeading, Vector3 cameraRotation, float cameraPitch, float targetSpeed, int imageNum=150){
            coords = playerCoords;
            heading = cameraHeading;
            rotation = cameraRotation;
            pitch = cameraPitch;
            speed = targetSpeed;
            targetImageNum = imageNum;

        }

        public void SetLocation(int player){
             //Set's players coords and camera position
            

            currentTime = 0;
            currentWeather = 0;
            imageCounter=0;
            shouldContinue = true;

            NetworkOverrideClockTime(times[currentTime], 00, 00);
            //SetOverrideWeather(weathers[currentWeather]);
            SetWeatherTypeNowPersist(weathers[currentWeather]);

            SetEntityCoords(player, coords.X, coords.Y, coords.Z, false, false, false, true);
            SetGameplayCamRelativeHeading(heading); 
            SetEntityRotation(player, rotation.X, rotation.Y, rotation.Z, 0, true);
            SetGameplayCamRelativePitch(pitch, 1f);
            

            //Set's first person mode
            SetFollowPedCamViewMode(4);

            FreezeEntityPosition(player, true);
            SetEntityInvincible(player, true);
                        
        }

        public void SetCondition(){// alwasys done before an image is taken 
            
            if(imageCounter>=targetImageNum){//if condition should be changed 
                if(currentTime!=(times.Length)-1){
                    //increase the time by one and reset image counter
                    currentTime+=1;
                    SetTime();
                    imageCounter=0;
                }else if(currentWeather!=(weathers.Length)-1){
                    //reset the current time to 0
                    currentTime=0;
                    SetTime();
                    // increase weather by one and reset image counter
                    currentWeather+=1;
                    //SetOverrideWeather(weathers[currentWeather]);
                    SetWeatherTypeNowPersist(weathers[currentWeather]);
                    imageCounter=0;
                }else{
                    shouldContinue=false;
                }
            //attempt to change time
            //if time can't be changed then attempt to change weather
            // if weather can't be changed then change the value of should continue 

            }
            
            imageCounter++; //Should this be in an else? Probably not


            // if the condition needs to be changed, change it :)
        }

        public void SetTime(){
            NetworkOverrideClockTime(times[currentTime], 00, 00);
        }

        public float GetSpeed(){
            return speed;
        }

        public bool getShouldContinue(){
            return shouldContinue;
        }

    }
}
