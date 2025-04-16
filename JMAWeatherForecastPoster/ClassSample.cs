namespace JMAWeatherForecastPoster
{
    internal class ClassSample
    {
        public class Json_Forecast
        {
            public class Rootobject
            {
                public Class1[] Property1 { get; set; }
            }

            public class Class1
            {
                public string publishingOffice { get; set; }
                public DateTime reportDatetime { get; set; }
                public Timesery[] timeSeries { get; set; }
                public Tempaverage tempAverage { get; set; }
                public Precipaverage precipAverage { get; set; }
            }

            public class Tempaverage
            {
                public Area[] areas { get; set; }
            }

            public class Area
            {
                public Area1 area { get; set; }
                public string min { get; set; }
                public string max { get; set; }
            }

            public class Area1
            {
                public string name { get; set; }
                public string code { get; set; }
            }

            public class Precipaverage
            {
                public Area2[] areas { get; set; }
            }

            public class Area2
            {
                public Area3 area { get; set; }
                public string min { get; set; }
                public string max { get; set; }
            }

            public class Area3
            {
                public string name { get; set; }
                public string code { get; set; }
            }

            public class Timesery
            {
                public DateTime[] timeDefines { get; set; }
                public Area4[] areas { get; set; }
            }

            public class Area4
            {
                public Area5 area { get; set; }
                public string[] weatherCodes { get; set; }
                public string[] weathers { get; set; }
                public string[] winds { get; set; }
                public string[] waves { get; set; }
                public string[] pops { get; set; }
                public string[] temps { get; set; }
                public string[] reliabilities { get; set; }
                public string[] tempsMin { get; set; }
                public string[] tempsMinUpper { get; set; }
                public string[] tempsMinLower { get; set; }
                public string[] tempsMax { get; set; }
                public string[] tempsMaxUpper { get; set; }
                public string[] tempsMaxLower { get; set; }
            }

            public class Area5
            {
                public string name { get; set; }
                public string code { get; set; }
            }
        }


        public class Json_Warning
        {
            public class Rootobject
            {
                public DateTime reportDatetime { get; set; }
                public string publishingOffice { get; set; }
                public string headlineText { get; set; }
                public string notice { get; set; }
                public Areatype[] areaTypes { get; set; }
                public Timesery[] timeSeries { get; set; }
            }

            public class Areatype
            {
                public Area[] areas { get; set; }
            }

            public class Area
            {
                public string code { get; set; }
                public Warning[] warnings { get; set; }
            }

            public class Warning
            {
                public string code { get; set; }
                public string status { get; set; }
            }

            public class Timesery
            {
                public DateTime[] timeDefines { get; set; }
                public Areatype1[] areaTypes { get; set; }
            }

            public class Areatype1
            {
                public Area1[] areas { get; set; }
            }

            public class Area1
            {
                public string code { get; set; }
                public Warning1[] warnings { get; set; }
            }

            public class Warning1
            {
                public string code { get; set; }
                public Level[] levels { get; set; }
                public Continuelevel[] continueLevels { get; set; }
            }

            public class Level
            {
                public string type { get; set; }
                public Localarea[] localAreas { get; set; }
            }

            public class Localarea
            {
                public string[] values { get; set; }
            }

            public class Continuelevel
            {
                public string type { get; set; }
                public Localarea1[] localAreas { get; set; }
            }

            public class Localarea1
            {
                public string value { get; set; }
            }
        }

        public class Json_Overview
        {
            public class Rootobject
            {
                public string publishingOffice { get; set; }
                public DateTime reportDatetime { get; set; }
                public string targetArea { get; set; }
                public string headlineText { get; set; }
                public string text { get; set; }
            }
        }
    }
}
