﻿// Copyright 2020 Serge Savelev
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using SSavel.V8Utils.Platform;

namespace SSavel.V8Utils.Windows.Platform.Com
{
    public static class ComConnectorFactory
    {
        public static IComConnector GetConnector(Version version)
        {
            if (Versions.Is82(version)) return new ComConnector82();

            if (Versions.Is83(version)) return new ComConnector83();

            throw new ArgumentOutOfRangeException(nameof(version));
        }
    }
}