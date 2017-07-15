﻿using OpenBve.BrakeSystems;

namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public static partial class TrainManager
	{
		/*
		 * Contains various specifications for the car
		 * TODO: Most of this should be merged into the root car specification.....
		 */
		internal struct CarSpecs
		{
			/// motor related properties
			
			///<summary>Whether this is a motor car</summary>
			internal bool IsMotorCar;

			internal bool IsDriverCar;
			/// <summary>The array of acceleration curve data</summary>
			internal AccelerationCurve[] AccelerationCurves;
			internal double AccelerationCurvesMultiplier;
			internal double AccelerationCurveMaximum;
			internal double JerkPowerUp;
			internal double JerkPowerDown;
			internal double JerkBrakeUp;
			internal double JerkBrakeDown;
			/// brake
			internal double BrakeDecelerationAtServiceMaximumPressure;
			internal double BrakeControlSpeed;
			internal double MotorDeceleration;
			/// physical properties
			
			///<summary>The car's empty mass in kg</summary>
			internal double MassEmpty;
			/// <summary>The car's current mass in kg</summary>
			internal double MassCurrent;
			internal double ExposedFrontalArea;
			internal double UnexposedFrontalArea;
			internal double CoefficientOfStaticFriction;
			internal double CoefficientOfRollingResistance;
			internal double AerodynamicDragCoefficient;
			internal double CenterOfGravityHeight;
			internal double CriticalTopplingAngle;
			/// current data
			internal double CurrentSpeed;
			internal double CurrentPerceivedSpeed;
			internal double CurrentPerceivedTraveledDistance;
			internal double CurrentAcceleration;
			/// <summary>The acceleration generated by the motor. Is positive for power and negative for brake, regardless of the train's direction.</summary>
			internal double CurrentAccelerationOutput;
			internal double CurrentRollDueToTopplingAngle;
			internal double CurrentRollDueToCantAngle;
			internal double CurrentRollDueToShakingAngle;
			internal double CurrentRollDueToShakingAngularSpeed;
			internal double CurrentRollShakeDirection;
			internal double CurrentPitchDueToAccelerationAngle;
			internal double CurrentPitchDueToAccelerationAngularSpeed;
			internal double CurrentPitchDueToAccelerationTargetAngle;
			internal double CurrentPitchDueToAccelerationFastValue;
			internal double CurrentPitchDueToAccelerationMediumValue;
			internal double CurrentPitchDueToAccelerationSlowValue;
			/// systems
			internal CarHoldBrake HoldBrake;
			//internal CarConstSpeed ConstSpeed;
			//internal CarReAdhesionDevice ReAdhesionDevice;
			internal CarBrakeType BrakeType;
			internal EletropneumaticBrakeType ElectropneumaticType;
			internal AirBrake.CarAirBrake AirBrake;
			/// doors
			internal Door[] Doors;
			internal double DoorOpenFrequency;
			internal double DoorCloseFrequency;
			internal double DoorOpenPitch;
			internal double DoorClosePitch;
			internal bool AnticipatedLeftDoorsOpened;
			internal bool AnticipatedRightDoorsOpened;
		}
	}
}
