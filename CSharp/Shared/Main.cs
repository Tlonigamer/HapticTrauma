using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Immutable;
using System.Text;
using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace HapticTrauma
{

    public partial class Main : IAssemblyPlugin
    {
        
        public void Initialize()
        {

            // Console.ReadLine();
            // // When your plugin is loading, use this instead of the constructor
            // // Put any code here that does not rely on other plugins.
            LuaCsLogger.Log("Haptictrauma Loaded");
        }

        public void OnLoadCompleted() {
            //Initialises values for later use
            float LastHealth = 100.0f;
            float CurrentHealth = 100.0f;
            float VibrationStrength = 0.0f;
            float Healthdelta = 0;

            // Adds a hook into the "Think" function, which runs 60 times per second
            GameMain.LuaCs.Hook.Add("Think", "VibrationManager", (object[] args) =>
            {
                if ((Character.Controlled != null) && !(Character.Controlled.IsDead)) //Check if the player is controlling an alive character
                {
                    if (Character.Controlled.InPressure && !Character.Controlled.IsProtectedFromPressure) //Is the character at risk of dying from water pressure?
                    {
                        //Vibrate with a strength of 0.5
                        LuaCsLogger.Log("Pressure!");
                        ContinuousVibrate(0.5f);
                    } else {
                        StopVibration();
                    }


                    if (Character.Controlled.IsUnconscious) //If the player is unconscious
                    {
                        LuaCsLogger.Log("Unconscious!");
                        ContinuousVibrate(0.1f);
                    } else {
                        StopVibration();
                    }


                    CurrentHealth = Character.Controlled.Health; //Get the player's current HP
                    if (CurrentHealth <= (LastHealth - 2)) //Has the player's HP decreased?
                    {
                        
                        Healthdelta = LastHealth - CurrentHealth; //Calculate the change in HP
                        LuaCsLogger.Log("Player was hit!");

                        //Set the vibration strength to be the change in hp divided by 20, a 2 HP difference results in a vibration strength of 0.1
                        VibrationStrength = Healthdelta / 20; 

                        //Prevent a vibration strength over 1, which is the max for Xna Vibration.
                        if (VibrationStrength > 1)
                        {
                            VibrationStrength = 1;
                        }

                        //Prevent a vibration strength under 0.1, which is the minumum for Xna Vibration.
                        else if (VibrationStrength < 0.1f)
                        {
                            VibrationStrength = 0.1f;
                        }
                        Vibrate(200, VibrationStrength);
                        LastHealth = Character.Controlled.Health;
                    }


                }
            //All hooks need to return null, no clue why, thanks Barotrauma Cs documentation for being vague as always
            return null;
            }
            );
        }

        //All Barotrauma mods need this, even if they do nothing
        public void PreInitPatching() {}
        public void Dispose() {}

        //Vibrates the controller with a set time and strength

        public async void Vibrate(int VibrationTime, float VibrationStrength)
        {
            //LuaCsLogger.Log("Vibrating!");
            //Set the vibration for 1st controller with a present strength
            GamePad.SetVibration(PlayerIndex.One, VibrationStrength, VibrationStrength);
            //Wait for the specified time
            await Task.Delay(VibrationTime);
            //Turn off motors
            GamePad.SetVibration(PlayerIndex.One, 0, 0);
                
        }
        public async void ContinuousVibrate(float VibrationStrength)
        {
            //LuaCsLogger.Log("Vibrating!");
            //Set the vibration for 1st controller with a present strength
            GamePad.SetVibration(PlayerIndex.One, VibrationStrength, VibrationStrength);
                
        }
        public async void StopVibration()
        {
            //LuaCsLogger.Log("Stopping Vibration!");
            //Stops vibration by setting motors to 0
            GamePad.SetVibration(PlayerIndex.One, 0.0f, 0.0f);
                
        }
    }
    
}
