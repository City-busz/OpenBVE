﻿using OpenBveApi.Math;

namespace OpenBve
{
	internal static partial class SoundCfgParser
	{
		/// <summary>Loads the sound set for a BVE2 based train</summary>
		/// <param name="train">The train</param>
		/// <param name="trainFolder">The absolute on-disk path to the train's folder</param>
		private static void LoadBve2Sounds(string trainFolder, TrainManager.Train train)
		{
			// set sound positions and radii
			Vector3 front = new Vector3(0.0, 0.0, 0.5 * train.Cars[train.DriverCar].Length);
			Vector3 center = new Vector3(0.0, 0.0, 0.0);
			Vector3 left = new Vector3(-1.3, 0.0, 0.0);
			Vector3 right = new Vector3(1.3, 0.0, 0.0);
			Vector3 cab = new Vector3(-train.Cars[train.DriverCar].DriverX, train.Cars[train.DriverCar].DriverY, train.Cars[train.DriverCar].DriverZ - 0.5);
			Vector3 panel = new Vector3(train.Cars[train.DriverCar].DriverX, train.Cars[train.DriverCar].DriverY, train.Cars[train.DriverCar].DriverZ + 1.0);
			train.InitializeCarSounds();
			// load sounds for driver's car
			train.Cars[train.DriverCar].Sounds.Adjust = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "Adjust.wav"), panel, tinyRadius);
			train.Cars[train.DriverCar].Sounds.Brake = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "Brake.wav"), center, smallRadius);
			train.Cars[train.DriverCar].Sounds.Halt = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "Halt.wav"), cab, tinyRadius);
			train.Cars[train.DriverCar].Sounds.Horns[0].LoopSound = Sounds.SoundBuffer.TryToLoad(OpenBveApi.Path.CombineFile(trainFolder, "Klaxon0.wav"), largeRadius);
			train.Cars[train.DriverCar].Sounds.Horns[0].Loop = false;
			train.Cars[train.DriverCar].Sounds.Horns[0].SoundPosition = front;
			if (train.Cars[train.DriverCar].Sounds.Horns[0].LoopSound == null)
			{
				train.Cars[train.DriverCar].Sounds.Horns[0].LoopSound = Sounds.SoundBuffer.TryToLoad(OpenBveApi.Path.CombineFile(trainFolder, "Klaxon.wav"), largeRadius);
			}
			train.Cars[train.DriverCar].Sounds.Horns[1].LoopSound = Sounds.SoundBuffer.TryToLoad(OpenBveApi.Path.CombineFile(trainFolder, "Klaxon1.wav"), largeRadius);
			train.Cars[train.DriverCar].Sounds.Horns[1].Loop = false;
			train.Cars[train.DriverCar].Sounds.Horns[1].SoundPosition = front;
			train.Cars[train.DriverCar].Sounds.Horns[2].LoopSound = Sounds.SoundBuffer.TryToLoad(OpenBveApi.Path.CombineFile(trainFolder, "Klaxon2.wav"), mediumRadius);
			train.Cars[train.DriverCar].Sounds.Horns[2].Loop = true;
			train.Cars[train.DriverCar].Sounds.Horns[2].SoundPosition = front;
			train.Cars[train.DriverCar].Sounds.PilotLampOn = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "Leave.wav"), cab, tinyRadius);
			train.Cars[train.DriverCar].Sounds.PilotLampOff = TrainManager.CarSound.Empty;
			// load sounds for all cars
			for (int i = 0; i < train.Cars.Length; i++)
			{
				Vector3 frontaxle = new Vector3(0.0, 0.0, train.Cars[i].FrontAxlePosition);
				Vector3 rearaxle = new Vector3(0.0, 0.0, train.Cars[i].RearAxlePosition);
				train.Cars[i].Sounds.Air = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "Air.wav"), center, smallRadius);
				train.Cars[i].Sounds.AirHigh = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "AirHigh.wav"), center, smallRadius);
				train.Cars[i].Sounds.AirZero = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "AirZero.wav"), center, smallRadius);
				if (train.Cars[i].Specs.AirBrake.Type == TrainManager.AirBrakeType.Main)
				{
					train.Cars[i].Sounds.CpEnd = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "CpEnd.wav"), center, mediumRadius);
					train.Cars[i].Sounds.CpLoop = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "CpLoop.wav"), center, mediumRadius);
					train.Cars[i].Sounds.CpStart = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "CpStart.wav"), center, mediumRadius);
				}
				train.Cars[i].Sounds.BreakerResume = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.BreakerResumeOrInterrupt = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.BreakerResumed = false;
				train.Cars[i].Sounds.DoorCloseL = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "DoorClsL.wav"), left, smallRadius);
				train.Cars[i].Sounds.DoorCloseR = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "DoorClsR.wav"), right, smallRadius);
				if (train.Cars[i].Sounds.DoorCloseL.Buffer == null)
				{
					train.Cars[i].Sounds.DoorCloseL = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "DoorCls.wav"), left, smallRadius);
				}
				if (train.Cars[i].Sounds.DoorCloseR.Buffer == null)
				{
					train.Cars[i].Sounds.DoorCloseR = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "DoorCls.wav"), right, smallRadius);
				}
				train.Cars[i].Sounds.DoorOpenL = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "DoorOpnL.wav"), left, smallRadius);
				train.Cars[i].Sounds.DoorOpenR = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "DoorOpnR.wav"), right, smallRadius);
				if (train.Cars[i].Sounds.DoorOpenL.Buffer == null)
				{
					train.Cars[i].Sounds.DoorOpenL = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "DoorOpn.wav"), left, smallRadius);
				}
				if (train.Cars[i].Sounds.DoorOpenR.Buffer == null)
				{
					train.Cars[i].Sounds.DoorOpenR = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "DoorOpn.wav"), right, smallRadius);
				}
				train.Cars[i].Sounds.EmrBrake = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "EmrBrake.wav"), center, mediumRadius);
				train.Cars[i].Sounds.Flange = TryLoadSoundArray(trainFolder, "Flange", ".wav", center, mediumRadius);
				train.Cars[i].Sounds.FlangeVolume = new double[train.Cars[i].Sounds.Flange.Length];
				train.Cars[i].Sounds.Loop = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "Loop.wav"), center, mediumRadius);
				train.Cars[i].Sounds.PointFrontAxle = new TrainManager.CarSound[]
				{
					new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "Point.wav"), frontaxle, smallRadius)
				};
				train.Cars[i].Sounds.PointRearAxle = new TrainManager.CarSound[]
				{
					new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "Point.wav"), rearaxle, smallRadius)
				};
				train.Cars[i].Sounds.Rub = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "Rub.wav"), center, mediumRadius);
				train.Cars[i].Sounds.Run = TryLoadSoundArray(trainFolder, "Run", ".wav", center, mediumRadius);
				train.Cars[i].Sounds.RunVolume = new double[train.Cars[i].Sounds.Run.Length];
				train.Cars[i].Sounds.SpringL = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "SpringL.wav"), left, smallRadius);
				train.Cars[i].Sounds.SpringR = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "SpringR.wav"), right, smallRadius);
				// motor sound
				if (train.Cars[i].Specs.IsMotorCar)
				{
					System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
					train.Cars[i].Sounds.Motor.Position = center;
					for (int j = 0; j < train.Cars[i].Sounds.Motor.Tables.Length; j++)
					{
						for (int k = 0; k < train.Cars[i].Sounds.Motor.Tables[j].Entries.Length; k++)
						{
							int idx = train.Cars[i].Sounds.Motor.Tables[j].Entries[k].SoundIndex;
							if (idx >= 0)
							{
								TrainManager.CarSound snd = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "Motor" + idx.ToString(Culture) + ".wav"), center, mediumRadius);
								train.Cars[i].Sounds.Motor.Tables[j].Entries[k].Buffer = snd.Buffer;
							}
						}
					}
				}
			}
		}
	}
}