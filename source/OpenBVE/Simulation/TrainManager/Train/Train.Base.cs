﻿using System;
using System.Windows.Forms;
using OpenBveApi.Colors;
using OpenBveApi.Math;

namespace OpenBve
{
	/*
	 * Contains the base definition of a train
	 */
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public static partial class TrainManager
	{
		/// <summary>The root class for a train within the simulation</summary>
		public class Train
		{
			/// <summary>The plugin used by this train.</summary>
			internal PluginManager.Plugin Plugin;
			internal int TrainIndex;
			internal TrainState State;
			internal Car[] Cars;
			internal Coupler[] Couplers;
			internal int DriverCar;
			internal TrainSpecs Specs;
		    internal BrakeSystems.EmergencyBrake EmergencyBrake;
			internal TrainPassengers Passengers;
			/// <summary>Holds various information on the previous and next stations</summary>
			internal StationInformation StationInfo;
			internal double[] RouteLimits;
			internal double CurrentRouteLimit;
			internal double CurrentSectionLimit;
			internal int CurrentSectionIndex;
			internal double TimetableDelta;
			internal Game.GeneralAI AI;
			internal double InternalTimerTimeElapsed;
			/// <summary>Stores whether any car in the train has derailed</summary>
			internal bool Derailed;

			/// <summary>Call this method to derail a single car within the train</summary>
			/// <param name="CarIndex">The car index to derail</param>
			/// <param name="ElapsedTime">The elapsed time for this frame (Used for logging)</param>
			internal void Derail(int CarIndex, double ElapsedTime)
			{
				this.Cars[CarIndex].Derailed = true;
				this.Derailed = true;
				if (Program.GenerateDebugLogging)
				{
					Program.AppendToLogFile("Train " + TrainIndex + ", Car " + CarIndex + " derailed. Current simulation time: " + Game.SecondsSinceMidnight + " Current frame time: " + ElapsedTime);
				}
			}

		    /// <summary>Call this method to derail the entire train</summary>
		    /// <param name="ElapsedTime">The elapsed time for this frame (Used for logging)</param>
		    internal void Derail(double ElapsedTime)
		    {
		        if (Program.GenerateDebugLogging)
		        {
		            Program.AppendToLogFile("Train " + TrainIndex + ", derailed. Current simulation time: " + Game.SecondsSinceMidnight + " Current frame time: " + ElapsedTime);
		        }
                for (int i = 0; i < Cars.Length; i++)
		        {
		            Cars[i].Derailed = true;
		        }
		    }

			/// <summary>Call this method to topple a car</summary>
			/// <param name="CarIndex">The car index to derail</param>
			/// <param name="ElapsedTime">The elapsed time for this frame (Used for logging)</param>
			internal void Topple(int CarIndex, double ElapsedTime)
			{
				this.Cars[CarIndex].Topples = true;
				if (Program.GenerateDebugLogging)
				{
					Program.AppendToLogFile("Train " + TrainIndex + ", Car " + CarIndex + " toppled. Current simulation time: " + Game.SecondsSinceMidnight + " Current frame time: " + ElapsedTime);
				}
			}

		    /// <summary>Call this method to topple the entire train</summary>
		    /// <param name="ElapsedTime">The elapsed time for this frame (Used for logging)</param>
		    internal void Topple(double ElapsedTime)
		    {
		        if (Program.GenerateDebugLogging)
		        {
		            Program.AppendToLogFile("Train " + TrainIndex + " toppled. Current simulation time: " + Game.SecondsSinceMidnight + " Current frame time: " + ElapsedTime);
		        }
                for (int i = 0; i < Cars.Length; i++)
		        {
		            Cars[i].Topples = true;
		        }
		    }

            /// <summary>Called once a frame to update the state of the train</summary>
            /// <param name="TimeElapsed">The frame time elapsed</param>
		    internal void Update(double TimeElapsed)
		    {
		        if (State == TrainState.Pending)
		        {
		            // pending train
		            bool forceIntroduction = this == PlayerTrain && !Game.MinimalisticSimulation;
		            double time = 0.0;
		            if (!forceIntroduction)
		            {
		                for (int i = 0; i < Game.Stations.Length; i++)
		                {
		                    if (Game.Stations[i].StopMode == Game.StationStopMode.AllStop | Game.Stations[i].StopMode == Game.StationStopMode.PlayerPass)
		                    {
		                        if (Game.Stations[i].ArrivalTime >= 0.0)
		                        {
		                            time = Game.Stations[i].ArrivalTime;
		                        }
		                        else if (Game.Stations[i].DepartureTime >= 0.0)
		                        {
		                            time = Game.Stations[i].DepartureTime - Game.Stations[i].StopTime;
		                        }
		                        break;
		                    }
		                }
		                time -= TimetableDelta;
		            }
		            if (Game.SecondsSinceMidnight >= time | forceIntroduction)
		            {
		                bool introduce = true;
		                if (!forceIntroduction)
		                {
		                    if (CurrentSectionIndex >= 0)
		                    {
		                        if (!Game.Sections[CurrentSectionIndex].IsFree())
		                        {
		                            introduce = false;
		                        }
		                    }
		                }
		                if (introduce)
		                {
		                    // train is introduced
		                    State = TrainState.Available;
		                    for (int j = 0; j < Cars.Length; j++)
		                    {
		                        if (Cars[j].CarSections.Length != 0)
		                        {
		                            Cars[j].ChangeCarSection(j <= DriverCar | this != PlayerTrain ? 0 : -1);
		                            Cars[j].FrontBogie.ChangeCarSection(this != PlayerTrain ? 0 : -1);
		                            Cars[j].RearBogie.ChangeCarSection(this != PlayerTrain ? 0 : -1);
		                        }
                                if (Cars[j].Specs.IsMotorCar)
		                        {
		                            if (Cars[j].Sounds.Loop.Buffer != null)
		                            {
		                                Vector3 pos = Cars[j].Sounds.Loop.Position;
		                                Cars[j].Sounds.Loop.Source = Sounds.PlaySound(Cars[j].Sounds.Loop.Buffer, 1.0, 1.0, pos, this, j, true);
		                            }
		                        }
		                    }
		                }
		            }
		        }
		        else if (State == TrainState.Available)
		        {
		            // available train
		            UpdateTrainPhysicsAndControls(this, TimeElapsed);
		            if (Interface.CurrentOptions.GameMode == Interface.GameMode.Arcade)
		            {
		                if (Specs.CurrentAverageSpeed > CurrentRouteLimit)
		                {
		                    Game.AddMessage(Interface.GetInterfaceString("message_route_overspeed"), MessageManager.MessageDependency.RouteLimit, Interface.GameMode.Arcade, MessageColor.Orange, double.PositiveInfinity, null);
		                }
		                if (CurrentSectionLimit == 0.0)
		                {
		                    Game.AddMessage(Interface.GetInterfaceString("message_signal_stop"), MessageManager.MessageDependency.SectionLimit, Interface.GameMode.Normal, MessageColor.Red, double.PositiveInfinity, null);
		                }
		                else if (Specs.CurrentAverageSpeed > CurrentSectionLimit)
		                {
		                    Game.AddMessage(Interface.GetInterfaceString("message_signal_overspeed"), MessageManager.MessageDependency.SectionLimit, Interface.GameMode.Normal, MessageColor.Orange, double.PositiveInfinity, null);
		                }
		            }
		            if (AI != null)
		            {
		                AI.Trigger(this, TimeElapsed);
		            }
		        }
		        else if (State == TrainState.Bogus)
		        {
		            // bogus train
		            if (AI != null)
		            {
		                AI.Trigger(this, TimeElapsed);
		            }
		        }
		    }

            internal void UpdateCabObjects()
		    {
		        Cars[0].UpdateObjects(0.0, true, false);
		    }
		    internal void UpdateObjects(double TimeElapsed, bool ForceUpdate)
		    {
		        if (!Game.MinimalisticSimulation)
		        {
		            for (int i = 0; i < Cars.Length; i++)
		            {
		                Cars[i].UpdateObjects(TimeElapsed, ForceUpdate, true);
		                Cars[i].FrontBogie.UpdateObjects(TimeElapsed, ForceUpdate);
		                Cars[i].RearBogie.UpdateObjects(TimeElapsed, ForceUpdate);
		            }
		        }
		    }

            /// <summary>Applies a power and / or brake notch</summary>
            /// <param name="PowerValue">The power notch to apply</param>
            /// <param name="PowerRelative">Whether this is relative to the current notch</param>
            /// <param name="BrakeValue">The brake notch to apply</param>
            /// <param name="BrakeRelative">Whether this is relative to the current notch</param>
		    internal void ApplyNotch(int PowerValue, bool PowerRelative, int BrakeValue, bool BrakeRelative)
		    {
		        //Determine the actual notch to be applied
		        int p = PowerRelative ? PowerValue + Specs.CurrentPowerNotch.Driver : PowerValue;
		        if (p < 0)
		        {
                    //Cannot have a power notch less than zero
		            p = 0;
		        }
		        else if (p > Specs.MaximumPowerNotch)
		        {
                    //Cannot have a power notch greater than available notches
		            p = Specs.MaximumPowerNotch;
		        }
		        int b = BrakeRelative ? BrakeValue + Specs.CurrentBrakeNotch.Driver : BrakeValue;
		        if (b < 0)
		        {
		            b = 0;
		        }
		        else if (b > Specs.MaximumBrakeNotch)
		        {
		            b = Specs.MaximumBrakeNotch;
		        }
		        // power sound
		        if (p < Specs.CurrentPowerNotch.Driver)
		        {
		            if (p > 0)
		            {
		                // down (not min)
		                Sounds.SoundBuffer buffer = Cars[DriverCar].Sounds.MasterControllerDown.Buffer;
		                if (buffer != null)
		                {
		                    Vector3 pos = Cars[DriverCar].Sounds.MasterControllerDown.Position;
		                    Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
		                }
		            }
		            else
		            {
		                // min
		                Sounds.SoundBuffer buffer = Cars[DriverCar].Sounds.MasterControllerMin.Buffer;
		                if (buffer != null)
		                {
		                    Vector3 pos = Cars[DriverCar].Sounds.MasterControllerMin.Position;
		                    Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
		                }
		            }
		        }
		        else if (p > Specs.CurrentPowerNotch.Driver)
		        {
		            if (p < Specs.MaximumPowerNotch)
		            {
		                // up (not max)
		                Sounds.SoundBuffer buffer = Cars[DriverCar].Sounds.MasterControllerUp.Buffer;
		                if (buffer != null)
		                {
		                    Vector3 pos = Cars[DriverCar].Sounds.MasterControllerUp.Position;
		                    Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
		                }
		            }
		            else
		            {
		                // max
		                Sounds.SoundBuffer buffer = Cars[DriverCar].Sounds.MasterControllerMax.Buffer;
		                if (buffer != null)
		                {
		                    Vector3 pos = Cars[DriverCar].Sounds.MasterControllerMax.Position;
		                    Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
		                }
		            }
		        }
		        // brake sound
		        if (b < Specs.CurrentBrakeNotch.Driver)
		        {
		            // brake release
		            Sounds.SoundBuffer buffer = Cars[DriverCar].Sounds.Brake.Buffer;
		            if (buffer != null)
		            {
		                Vector3 pos = Cars[DriverCar].Sounds.Brake.Position;
		                Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
		            }
		            if (b > 0)
		            {
		                // brake release (not min)
		                buffer = Cars[DriverCar].Sounds.BrakeHandleRelease.Buffer;
		                if (buffer != null)
		                {
		                    Vector3 pos = Cars[DriverCar].Sounds.BrakeHandleRelease.Position;
		                    Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
		                }
		            }
		            else
		            {
		                // brake min
		                buffer = Cars[DriverCar].Sounds.BrakeHandleMin.Buffer;
		                if (buffer != null)
		                {
		                    Vector3 pos = Cars[DriverCar].Sounds.BrakeHandleMin.Position;
		                    Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
		                }
		            }
		        }
		        else if (b > Specs.CurrentBrakeNotch.Driver)
		        {
		            // brake
		            Sounds.SoundBuffer buffer = Cars[DriverCar].Sounds.BrakeHandleApply.Buffer;
		            if (buffer != null)
		            {
		                Vector3 pos = Cars[DriverCar].Sounds.BrakeHandleApply.Position;
		                Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
		            }
		        }
		        // apply notch
		        if (Specs.SingleHandle)
		        {
                    //If this is a combined power/ brake handle train, then with a non-zero brake notch
                    //the power notch MUST be zero
		            if (b != 0) p = 0;
		        }
		        Specs.CurrentPowerNotch.Driver = p;
		        Specs.CurrentBrakeNotch.Driver = b;
		        Game.AddBlackBoxEntry(Game.BlackBoxEventToken.None);
		        // plugin
		        if (Plugin != null)
		        {
		            Plugin.UpdatePower();
		            Plugin.UpdateBrake();
		        }
		    }

            /// <summary>Call this method to move a car</summary>
            /// <param name="CarIndex">The car index to move</param>
            /// <param name="Delta">The length to move</param>
            /// <param name="TimeElapsed">The elapsed time</param>
            internal void MoveCar(int CarIndex, double Delta, double TimeElapsed)
			{
				if (State != TrainState.Disposed)
				{
					TrackManager.UpdateTrackFollower(ref Cars[CarIndex].FrontAxle.Follower, Cars[CarIndex].FrontAxle.Follower.TrackPosition + Delta, true, true);
					TrackManager.UpdateTrackFollower(ref Cars[CarIndex].FrontBogie.FrontAxle.Follower, Cars[CarIndex].FrontBogie.FrontAxle.Follower.TrackPosition + Delta, true, true);
					TrackManager.UpdateTrackFollower(ref Cars[CarIndex].FrontBogie.RearAxle.Follower, Cars[CarIndex].FrontBogie.RearAxle.Follower.TrackPosition + Delta, true, true);
					if (State != TrainState.Disposed)
					{
						TrackManager.UpdateTrackFollower(ref Cars[CarIndex].RearAxle.Follower, Cars[CarIndex].RearAxle.Follower.TrackPosition + Delta, true, true);
						TrackManager.UpdateTrackFollower(ref Cars[CarIndex].RearBogie.FrontAxle.Follower, Cars[CarIndex].RearBogie.FrontAxle.Follower.TrackPosition + Delta, true, true);
						TrackManager.UpdateTrackFollower(ref Cars[CarIndex].RearBogie.RearAxle.Follower, Cars[CarIndex].RearBogie.RearAxle.Follower.TrackPosition + Delta, true, true);
						if (State != TrainState.Disposed)
						{
							TrackManager.UpdateTrackFollower(ref Cars[CarIndex].BeaconReceiver, Cars[CarIndex].BeaconReceiver.TrackPosition + Delta, true, true);
						}
					}
				}
			}

			/// <summary>Call this method to place the cars of a train</summary>
			/// <param name="TrackPosition">The track position to start from</param>
			internal void PlaceCars(double TrackPosition)
			{
				for (int i = 0; i < Cars.Length; i++)
				{
					//Front axle track position
					Cars[i].FrontAxle.Follower.TrackPosition = TrackPosition - 0.5 * Cars[i].Length + Cars[i].FrontAxle.Position;
					//Bogie for front axle
					Cars[i].FrontBogie.FrontAxle.Follower.TrackPosition = Cars[i].FrontAxle.Follower.TrackPosition - 0.5 * Cars[i].FrontBogie.Length + Cars[i].FrontBogie.FrontAxle.Position;
					Cars[i].FrontBogie.RearAxle.Follower.TrackPosition = Cars[i].FrontAxle.Follower.TrackPosition - 0.5 * Cars[i].FrontBogie.Length + Cars[i].FrontBogie.RearAxlePosition;
					//Rear axle track position
					Cars[i].RearAxle.Follower.TrackPosition = TrackPosition - 0.5 * Cars[i].Length + Cars[i].RearAxle.Position;
					//Bogie for rear axle
					Cars[i].RearBogie.FrontAxle.Follower.TrackPosition = Cars[i].RearAxle.Follower.TrackPosition - 0.5 * Cars[i].RearBogie.Length + Cars[i].RearBogie.FrontAxle.Position;
					Cars[i].RearBogie.RearAxle.Follower.TrackPosition = Cars[i].RearAxle.Follower.TrackPosition - 0.5 * Cars[i].RearBogie.Length + Cars[i].RearBogie.RearAxlePosition;
					//Beacon reciever (AWS, ATC etc.)
					Cars[i].BeaconReceiver.TrackPosition = TrackPosition - 0.5 * Cars[i].Length + Cars[i].BeaconReceiverPosition;
					TrackPosition -= Cars[i].Length;
					if (i < Cars.Length - 1)
					{
						TrackPosition -= 0.5 * (Couplers[i].MinimumDistanceBetweenCars + Couplers[i].MaximumDistanceBetweenCars);
					}
				}
			}

            /// <summary>Call this method to load the exterior for a train</summary>
            /// <param name="exteriorFolder">The absolute on-disk path of the exterior folder</param>
            /// <param name="textEncoding"></param>
			internal void LoadExterior(string exteriorFolder, System.Text.Encoding textEncoding)
			{
				ObjectManager.UnifiedObject[] CarObjects;
				ObjectManager.UnifiedObject[] BogieObjects;
				ExtensionsCfgParser.ParseExtensionsConfig(exteriorFolder, textEncoding, out CarObjects, out BogieObjects, this);
				System.Threading.Thread.Sleep(1);
				//Stores the current array index of the bogie object to add
				//Required as there are two bogies per car, and we're using a simple linear array....
				int currentBogieObject = 0;
				for (int i = 0; i < Cars.Length; i++)
				{
					if (CarObjects[i] == null)
					{
						// load default exterior object
						string file = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("Compatibility"), "exterior.csv");
						ObjectManager.StaticObject so = ObjectManager.LoadStaticObject(file, System.Text.Encoding.UTF8, ObjectManager.ObjectLoadMode.Normal, false, false, false);
						if (so == null)
						{
							CarObjects[i] = null;
						}
						else
						{
							double sx = Cars[i].Width;
							double sy = Cars[i].Height;
							double sz = Cars[i].Length;
							CsvB3dObjectParser.ApplyScale(so, sx, sy, sz);
							CarObjects[i] = so;
						}
					}
					if (CarObjects[i] != null)
					{
						// add object
						int j = Cars[i].CarSections.Length;
						Array.Resize<TrainManager.CarSection>(ref Cars[i].CarSections, j + 1);
                        Cars[i].CarSections[j] = new CarSection();
						if (CarObjects[i] is ObjectManager.StaticObject)
						{
							ObjectManager.StaticObject s = (ObjectManager.StaticObject)CarObjects[i];
							Cars[i].CarSections[j].Elements = new ObjectManager.AnimatedObject[1];
							Cars[i].CarSections[j].Elements[0] = new ObjectManager.AnimatedObject
							{
								States = new ObjectManager.AnimatedObjectState[1]
							};
							Cars[i].CarSections[j].Elements[0].States[0].Position = new Vector3(0.0, 0.0, 0.0);
							Cars[i].CarSections[j].Elements[0].States[0].Object = s;
							Cars[i].CarSections[j].Elements[0].CurrentState = 0;
							Cars[i].CarSections[j].Elements[0].ObjectIndex = ObjectManager.CreateDynamicObject();
						}
						else if (CarObjects[i] is ObjectManager.AnimatedObjectCollection)
						{
							ObjectManager.AnimatedObjectCollection a = (ObjectManager.AnimatedObjectCollection)CarObjects[i];
							Cars[i].CarSections[j].Elements = new ObjectManager.AnimatedObject[a.Objects.Length];
							for (int h = 0; h < a.Objects.Length; h++)
							{
								Cars[i].CarSections[j].Elements[h] = a.Objects[h];
								Cars[i].CarSections[j].Elements[h].ObjectIndex = ObjectManager.CreateDynamicObject();
							}
						}
					}

					//Load bogie objects
					if (BogieObjects[currentBogieObject] != null)
					{
						int j = Cars[i].FrontBogie.CarSections.Length;
						Array.Resize<TrainManager.CarSection>(ref Cars[i].FrontBogie.CarSections, j + 1);
                        Cars[i].FrontBogie.CarSections[j] = new CarSection();
						if (BogieObjects[currentBogieObject] is ObjectManager.StaticObject)
						{
							ObjectManager.StaticObject s = (ObjectManager.StaticObject)BogieObjects[currentBogieObject];
							Cars[i].FrontBogie.CarSections[j].Elements = new ObjectManager.AnimatedObject[1];
							Cars[i].FrontBogie.CarSections[j].Elements[0] = new ObjectManager.AnimatedObject();
							Cars[i].FrontBogie.CarSections[j].Elements[0].States = new ObjectManager.AnimatedObjectState[1];
							Cars[i].FrontBogie.CarSections[j].Elements[0].States[0].Position = new Vector3(0.0, 0.0, 0.0);
							Cars[i].FrontBogie.CarSections[j].Elements[0].States[0].Object = s;
							Cars[i].FrontBogie.CarSections[j].Elements[0].CurrentState = 0;
							Cars[i].FrontBogie.CarSections[j].Elements[0].ObjectIndex = ObjectManager.CreateDynamicObject();
						}
						else if (BogieObjects[currentBogieObject] is ObjectManager.AnimatedObjectCollection)
						{
							ObjectManager.AnimatedObjectCollection a = (ObjectManager.AnimatedObjectCollection)BogieObjects[currentBogieObject];
							Cars[i].FrontBogie.CarSections[j].Elements = new ObjectManager.AnimatedObject[a.Objects.Length];
							for (int h = 0; h < a.Objects.Length; h++)
							{
								Cars[i].FrontBogie.CarSections[j].Elements[h] = a.Objects[h];
								Cars[i].FrontBogie.CarSections[j].Elements[h].ObjectIndex = ObjectManager.CreateDynamicObject();
							}
						}
					}
					currentBogieObject++;
					//Can't think of a better way to do this than two functions......
					if (BogieObjects[currentBogieObject] != null)
					{
						int j = Cars[i].RearBogie.CarSections.Length;
						Array.Resize<TrainManager.CarSection>(ref Cars[i].RearBogie.CarSections, j + 1);
                        Cars[i].RearBogie.CarSections[j] = new CarSection();
						if (BogieObjects[currentBogieObject] is ObjectManager.StaticObject)
						{
							ObjectManager.StaticObject s = (ObjectManager.StaticObject)BogieObjects[currentBogieObject];
							Cars[i].RearBogie.CarSections[j].Elements = new ObjectManager.AnimatedObject[1];
							Cars[i].RearBogie.CarSections[j].Elements[0] = new ObjectManager.AnimatedObject
							{
								States = new ObjectManager.AnimatedObjectState[1]
							};
							Cars[i].RearBogie.CarSections[j].Elements[0].States[0].Position = new Vector3(0.0, 0.0, 0.0);
							Cars[i].RearBogie.CarSections[j].Elements[0].States[0].Object = s;
							Cars[i].RearBogie.CarSections[j].Elements[0].CurrentState = 0;
							Cars[i].RearBogie.CarSections[j].Elements[0].ObjectIndex = ObjectManager.CreateDynamicObject();
						}
						else if (BogieObjects[currentBogieObject] is ObjectManager.AnimatedObjectCollection)
						{
							ObjectManager.AnimatedObjectCollection a = (ObjectManager.AnimatedObjectCollection)BogieObjects[currentBogieObject];
							Cars[i].RearBogie.CarSections[j].Elements = new ObjectManager.AnimatedObject[a.Objects.Length];
							for (int h = 0; h < a.Objects.Length; h++)
							{
								Cars[i].RearBogie.CarSections[j].Elements[h] = a.Objects[h];
								Cars[i].RearBogie.CarSections[j].Elements[h].ObjectIndex = ObjectManager.CreateDynamicObject();
							}
						}
					}
					currentBogieObject++;
				}
			}

		    /// <summary>Attempts to load and parse the current train's panel configuration file.</summary>
		    /// <param name="TrainPath">The absolute on-disk path to the train folder.</param>
		    /// <param name="Encoding">The automatically detected or manually set encoding of the panel configuration file.</param>
		    internal void ParsePanelConfig(string TrainPath, System.Text.Encoding Encoding)
		    {
		        string File = OpenBveApi.Path.CombineFile(TrainPath, "panel.animated");
		        if (System.IO.File.Exists(File))
		        {
		            Program.AppendToLogFile("Loading train panel: " + File);
		            ObjectManager.AnimatedObjectCollection a = AnimatedObjectParser.ReadObject(File, Encoding, ObjectManager.ObjectLoadMode.DontAllowUnloadOfTextures);
		            try
		            {
		                for (int i = 0; i < a.Objects.Length; i++)
		                {
		                    a.Objects[i].ObjectIndex = ObjectManager.CreateDynamicObject();
		                }
		                Cars[DriverCar].CarSections[0].Elements = a.Objects;
		                World.CameraRestriction = World.CameraRestrictionMode.NotAvailable;
		            }
		            catch
		            {
		                var currentError = Interface.GetInterfaceString("error_critical_file");
		                currentError = currentError.Replace("[file]", "panel.animated");
		                MessageBox.Show(currentError, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
		                Program.RestartArguments = " ";
		                Loading.Cancel = true;
		            }
		        }
		        else
		        {
		            var Panel2 = false;
		            try
		            {
		                File = OpenBveApi.Path.CombineFile(TrainPath, "panel2.cfg");
		                if (System.IO.File.Exists(File))
		                {
		                    Program.AppendToLogFile("Loading train panel: " + File);
		                    Panel2 = true;
		                    Panel2CfgParser.ParsePanel2Config(TrainPath, Encoding, this);
		                    World.CameraRestriction = World.CameraRestrictionMode.On;
		                }
		                else
		                {
		                    File = OpenBveApi.Path.CombineFile(TrainPath, "panel.cfg");
		                    if (System.IO.File.Exists(File))
		                    {
		                        Program.AppendToLogFile("Loading train panel: " + File);
		                        PanelCfgParser.ParsePanelConfig(TrainPath, Encoding, this);
		                        World.CameraRestriction = World.CameraRestrictionMode.On;
		                    }
		                    else
		                    {
		                        World.CameraRestriction = World.CameraRestrictionMode.NotAvailable;
		                    }
		                }
		            }
		            catch
		            {
		                var currentError = Interface.GetInterfaceString("errors_critical_file");
		                currentError = currentError.Replace("[file]", Panel2 == true ? "panel2.cfg" : "panel.cfg");
		                MessageBox.Show(currentError, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
		                Program.RestartArguments = " ";
		                Loading.Cancel = true;
		            }

		        }
		    }

            /// <summary>Updates the atmospheric constants applying to this train (Effects brake pressure speeds etc.)</summary>
            internal void UpdateAtmosphericConstants()
			{
				double h = 0.0;
				for (int i = 0; i < Cars.Length; i++)
				{
					h += Cars[i].FrontAxle.Follower.WorldPosition.Y + Cars[i].RearAxle.Follower.WorldPosition.Y;
				}
				Specs.CurrentElevation = Game.RouteInitialElevation + h / (2.0 * (double)Cars.Length);
				Specs.CurrentAirTemperature = Game.GetAirTemperature(Specs.CurrentElevation);
				Specs.CurrentAirPressure = Game.GetAirPressure(Specs.CurrentElevation, Specs.CurrentAirTemperature);
				Specs.CurrentAirDensity = Game.GetAirDensity(Specs.CurrentAirPressure, Specs.CurrentAirTemperature);
			}

			/// <summary>If this train is currently non-visible (minimalistic simulation) used to update the position of all cars infrequently</summary>
			internal void Synchronize()
			{
				for (int i = 0; i < Cars.Length; i++)
				{
					double s = 0.5 * (Cars[i].FrontAxle.Follower.TrackPosition + Cars[i].RearAxle.Follower.TrackPosition);
					double d = 0.5 * (Cars[i].FrontAxle.Follower.TrackPosition - Cars[i].RearAxle.Follower.TrackPosition);
					TrackManager.UpdateTrackFollower(ref Cars[i].FrontAxle.Follower, s + d, false, false);
					TrackManager.UpdateTrackFollower(ref Cars[i].RearAxle.Follower, s - d, false, false);
					double b = Cars[i].FrontAxle.Follower.TrackPosition - Cars[i].FrontAxle.Position + Cars[i].BeaconReceiverPosition;
					TrackManager.UpdateTrackFollower(ref Cars[i].BeaconReceiver, b, false, false);
				}
			}

			/// <summary>Updates this train's safety system plugin</summary>
			internal void UpdateSafetySystem()
			{
				Game.UpdatePluginSections(this);
				if (Plugin != null)
				{
					Plugin.LastSection = CurrentSectionIndex;
					Plugin.UpdatePlugin();
				}
			}


		    internal void Initialize()
		    {
		        for (int i = 0; i < Cars.Length; i++)
		        {
		            Cars[i].Initialize();
		        }
		        UpdateAtmosphericConstants();
		        Update(0.0);
		    }

            /// <summary>Disposes of this train</summary>
            internal void Dispose()
		    {
		        State = TrainState.Disposed;
		        for (int i = 0; i < Cars.Length; i++)
		        {
		            int s = Cars[i].CurrentCarSection;
		            if (s >= 0)
		            {
		                for (int j = 0; j < Cars[i].CarSections[s].Elements.Length; j++)
		                {
		                    Renderer.HideObject(Cars[i].CarSections[s].Elements[j].ObjectIndex);
		                }
		            }
		            s = Cars[i].FrontBogie.CurrentCarSection;
		            if (s >= 0)
		            {
		                for (int j = 0; j < Cars[i].FrontBogie.CarSections[s].Elements.Length; j++)
		                {
		                    Renderer.HideObject(Cars[i].FrontBogie.CarSections[s].Elements[j].ObjectIndex);
		                }
		            }
		            s = Cars[i].RearBogie.CurrentCarSection;
		            if (s >= 0)
		            {
		                for (int j = 0; j < Cars[i].RearBogie.CarSections[s].Elements.Length; j++)
		                {
		                    Renderer.HideObject(Cars[i].RearBogie.CarSections[s].Elements[j].ObjectIndex);
		                }
		            }
		        }
		        Sounds.StopAllSounds(this);

		        for (int i = 0; i < Game.Sections.Length; i++)
		        {
		            Game.Sections[i].Leave(this);
		        }
		        if (Game.Sections.Length != 0)
		        {
		            Game.UpdateSection(Game.Sections.Length - 1);
		        }
		    }
        }
	}
}
