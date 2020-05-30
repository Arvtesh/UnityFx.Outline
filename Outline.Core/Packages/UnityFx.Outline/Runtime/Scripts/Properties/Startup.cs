// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

#if !UNITY_2018_4_OR_NEWER
#error UnityFx.Outline requires Unity 2018.4 or newer.
#endif

#if NET_LEGACY || NET_2_0 || NET_2_0_SUBSET
#error UnityFx.Outline does not support .NET 3.5. Please set Scripting Runtime Version to .NET 4.x Equivalent in Unity Player Settings.
#endif
