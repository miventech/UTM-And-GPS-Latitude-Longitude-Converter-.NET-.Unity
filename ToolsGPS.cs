using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class ToolsGPS : MonoBehaviour
{
    /* global UTMLatLng */

    public static string datumName = "WGS 84"; // metricas
    public static double a = 6378137.0; // semi-major axis of ellipsoid in meters
    public static double eccSquared = 0.00669438; // eccentricity squared
    public static bool status = false; // true if datum is valid

    
    /// <summary>
    /// this function convert coodenates UTM to lat and lon
    /// </summary>
    /// <param name="utmX"></param>
    /// <param name="utmY"></param>
    /// <param name="utmZone"></param>
    /// <param name="latitude"></param>
    /// <param name="longitude"></param>
    public static void UTMToLatLon(double utmX, double utmY, string utmZone, out double latitude, out double longitude)
    {
        Debug.Log(utmZone[utmZone.Length-1]);
        char zoneLetter = utmZone[utmZone.Length-1];
        bool isNorthHemisphere = CheckIsNorthHemisphere(zoneLetter.ToString());
        var diflat = -0.00066286966871111111111111111111111111;
        var diflon = -0.0003868060578;
        var zone = int.Parse(utmZone.Remove(utmZone.Length - 1));
        var c_sa = 6378137.000000;
        var c_sb = 6356752.314245;
        var e2 = Math.Pow((Math.Pow(c_sa,2) - Math.Pow(c_sb,2)),0.5)/c_sb;
        var e2cuadrada = Math.Pow(e2,2);
        var c = Math.Pow(c_sa,2) / c_sb;
        var x = utmX - 500000;
        var y = isNorthHemisphere ? utmY : utmY - 10000000;
        var s = ((zone * 6.0) - 183.0);
        var lat = y / (c_sa * 0.9996);
        var v = (c / Math.Pow(1 + (e2cuadrada * Math.Pow(Math.Cos(lat), 2)), 0.5)) * 0.9996;
        var a = x / v;
        var a1 = Math.Sin(2 * lat);
        var a2 = a1 * Math.Pow((Math.Cos(lat)), 2);
        var j2 = lat + (a1 / 2.0);
        var j4 = ((3 * j2) + a2) / 4.0;
        var j6 = ((5 * j4) + Math.Pow(a2 * (Math.Cos(lat)), 2)) / 3.0;
        var alfa = (3.0 / 4.0) * e2cuadrada;
        var beta = (5.0 / 3.0) * Math.Pow(alfa, 2);
        var gama = (35.0 / 27.0) * Math.Pow(alfa, 3);
        var bm = 0.9996 * c * (lat - alfa * j2 + beta * j4 - gama * j6);
        var b = (y - bm) / v;
        var epsi = ((e2cuadrada * Math.Pow(a, 2)) / 2.0) * Math.Pow((Math.Cos(lat)), 2);
        var eps = a * (1 - (epsi / 3.0));
        var nab = (b * (1 - epsi)) + lat;
        var senoheps = (Math.Exp(eps) - Math.Exp(-eps)) / 2.0;
        var delt  = Math.Atan(senoheps/(Math.Cos(nab) ) );
        var tao = Math.Atan(Math.Cos(delt) * Math.Tan(nab));
        longitude = ((delt * (180.0 / Math.PI)) + s) + diflon;
        latitude = ((lat + (1 + e2cuadrada * Math.Pow(Math.Cos(lat), 2) - (3.0 / 2.0) * e2cuadrada * Math.Sin(lat) * Math.Cos(lat) * (tao - lat)) * (tao - lat)) * (180.0 / Math.PI)) + diflat;
    }



    /// <summary>
    /// this function convert coodenates lat and lon to UTM
    /// </summary>
    /// <param name="latitude"></param>
    /// <param name="longitude"></param>
    /// <param name="utmX"></param>
    /// <param name="utmY"></param>
    /// <param name="ZoneNumber"></param>
    /// <param name="ZoneLetter"></param>
    void convertLatLonToUtm(double latitude, double longitude , ref double utmX, ref double utmY, ref int ZoneNumber , ref string ZoneLetter)
    {

    var LongTemp = longitude;
    var LatRad = toRadians(latitude);
    var LongRad = toRadians(LongTemp);
    if (LongTemp >= 8 && LongTemp <= 13 && latitude > 54.5 && latitude < 58) {
        ZoneNumber = 32;
    } else if (latitude >= 56.0 && latitude < 64.0 && LongTemp >= 3.0 && LongTemp < 12.0) {
        ZoneNumber = 32;
    } else {
        ZoneNumber = (int)(((LongTemp + 180) / 6) + 1);

        if (latitude >= 72.0 && latitude < 84.0) {
            if (LongTemp >= 0.0 && LongTemp < 9.0) {
                ZoneNumber = 31;
            } else if (LongTemp >= 9.0 && LongTemp < 21.0) {
                ZoneNumber = 33;
            } else if (LongTemp >= 21.0 && LongTemp < 33.0) {
                ZoneNumber = 35;
            } else if (LongTemp >= 33.0 && LongTemp < 42.0) {
                ZoneNumber = 37;
            }
        }
    }

    var LongOrigin = (ZoneNumber - 1) * 6 - 180 + 3;  //+3 puts origin in middle of zone
    var LongOriginRad = toRadians(LongOrigin);

    var UTMZone = getUtmLetterDesignator(latitude);

    var eccPrimeSquared = (eccSquared) / (1 - eccSquared);

    var N = a / Math.Sqrt(1 - eccSquared * Math.Sin(LatRad) * Math.Sin(LatRad));
    var T = Math.Tan(LatRad) * Math.Tan(LatRad);
    var C = eccPrimeSquared * Math.Cos(LatRad) * Math.Cos(LatRad);
    var A = Math.Cos(LatRad) * (LongRad - LongOriginRad);

    var M = a * ((1 - eccSquared / 4 - 3 * eccSquared * eccSquared / 64 - 5 * eccSquared * eccSquared * eccSquared / 256) * LatRad
        - (3 * eccSquared / 8 + 3 * eccSquared * eccSquared / 32 + 45 * eccSquared * eccSquared * eccSquared / 1024) * Math.Sin(2 * LatRad)
        + (15 * eccSquared * eccSquared / 256 + 45 * eccSquared * eccSquared * eccSquared / 1024) * Math.Sin(4 * LatRad)
        - (35 * eccSquared * eccSquared * eccSquared / 3072) * Math.Sin(6 * LatRad));

    var UTMEasting = (0.9996 * N * (A + (1 - T + C) * A * A * A / 6
        + (5 - 18 * T + T * T + 72 * C - 58 * eccPrimeSquared) * A * A * A * A * A / 120)
    + 500000.0);

    var UTMNorthing = (0.9996 * (M + N * Math.Tan(LatRad) * (A * A / 2 + (5 - T + 9 * C + 4 * C * C) * A * A * A * A / 24
        + (61 - 58 * T + T * T + 600 * C - 330 * eccPrimeSquared) * A * A * A * A * A * A / 720)));

    if (latitude < 0)
        UTMNorthing += 10000000.0; //10000000 meter offset for southern hemisphere
    utmX = UTMEasting;
    utmY = UTMNorthing;
    ZoneLetter = UTMZone;
}


/// <summary>
/// Check is the latitude is for the southern hemisphere
/// </summary>
/// <param name="letterZone"></param>
/// <returns></returns>
public static bool CheckIsNorthHemisphere(string letterZone){
    letterZone = letterZone.ToUpperInvariant();
    char latterCharZone = letterZone[0];
    char[] ListzoneLetters = new char[]{'N', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'};
    foreach (var Letter in ListzoneLetters)
    {
        if (latterCharZone == Letter)
        {
            return true;
        }
    }
    return false;
}
/// <summary>
/// this function convert UTM to lat and lon coordenates
/// </summary>
/// <param name="UTMEasting"></param>
/// <param name="UTMNorthing"></param>
/// <param name="UTMZoneNumber"></param>
/// <param name="UTMZoneLetter"></param>
public static void convertUtmToLatLng(double UTMEasting, double UTMNorthing, int UTMZoneNumber, char UTMZoneLetter)
{
    var e1 = (1 - Math.Sqrt(1 - eccSquared)) / (1 + Math.Sqrt(1 - eccSquared));
    var x = UTMEasting - 500000.0; //remove 500,000 meter offset for longitude
    var y = UTMNorthing;
    var ZoneNumber = UTMZoneNumber;
    var ZoneLetter = UTMZoneLetter;
    char[] ListzoneLetters = new char[]{'N', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'};
    bool zoneLetterFound = CheckIsNorthHemisphere(UTMZoneLetter.ToString());
    y = (zoneLetterFound) ? y : y - 10000000.0;

    double LongOrigin = (ZoneNumber - 1) * 6 - 180 + 3;  

    double eccPrimeSquared = (eccSquared) / (1 - eccSquared);

    double M = y / 0.9996;
    double mu = M / (a * (1 - eccSquared / 4 - 3 * eccSquared * eccSquared / 64 - 5 * eccSquared * eccSquared * eccSquared / 256));

    double phi1Rad = mu + (3 * e1 / 2 - 27 * e1 * e1 * e1 / 32) * Math.Sin(2 * mu)
    + (21 * e1 * e1 / 16 - 55 * e1 * e1 * e1 * e1 / 32) * Math.Sin(4 * mu)
    + (151 * e1 * e1 * e1 / 96) * Math.Sin(6 * mu);
    var phi1 = toDegrees(phi1Rad);

    var N1 = a / Math.Sqrt(1 - eccSquared * Math.Sin(phi1Rad) * Math.Sin(phi1Rad));
    var T1 = Math.Tan(phi1Rad) * Math.Tan(phi1Rad);
    var C1 = eccPrimeSquared * Math.Cos(phi1Rad) * Math.Cos(phi1Rad);
    var R1 = a * (1 - eccSquared) / Math.Pow(1 - eccSquared * Math.Sin(phi1Rad) * Math.Sin(phi1Rad), 1.5);
    var D = x / (N1 * 0.9996);

    var Lat = phi1Rad - (N1 * Math.Tan(phi1Rad) / R1) * (D * D / 2 - (5 + 3 * T1 + 10 * C1 - 4 * C1 * C1 - 9 * eccPrimeSquared) * D * D * D * D / 24
        + (61 + 90 * T1 + 298 * C1 + 45 * T1 * T1 - 252 * eccPrimeSquared - 3 * C1 * C1) * D * D * D * D * D * D / 720);
    Lat = toDegrees(Lat);

    var Long = (D - (1 + 2 * T1 + C1) * D * D * D / 6 + (5 - 2 * C1 + 28 * T1 - 3 * C1 * C1 + 8 * eccPrimeSquared + 24 * T1 * T1)
        * D * D * D * D * D / 120) / Math.Cos(phi1Rad);
    Long = LongOrigin + toDegrees(Long);
}

/// <summary>
/// get latter zone letter from latitude
/// </summary>
/// <param name="latitude"></param>
/// <returns></returns>
public static string getUtmLetterDesignator(double latitude) {
    if ((84 >= latitude) && (latitude >= 72))
        return "X";
    else if ((72 > latitude) && (latitude >= 64))
        return "W";
    else if ((64 > latitude) && (latitude >= 56))
        return "V";
    else if ((56 > latitude) && (latitude >= 48))
        return "U";
    else if ((48 > latitude) && (latitude >= 40))
        return "T";
    else if ((40 > latitude) && (latitude >= 32))
        return "S";
    else if ((32 > latitude) && (latitude >= 24))
        return "R";
    else if ((24 > latitude) && (latitude >= 16))
        return "Q";
    else if ((16 > latitude) && (latitude >= 8))
        return "P";
    else if ((8 > latitude) && (latitude >= 0))
        return "N";
    else if ((0 > latitude) && (latitude >= -8))
        return "M";
    else if ((-8 > latitude) && (latitude >= -16))
        return "L";
    else if ((-16 > latitude) && (latitude >= -24))
        return "K";
    else if ((-24 > latitude) && (latitude >= -32))
        return "J";
    else if ((-32 > latitude) && (latitude >= -40))
        return "H";
    else if ((-40 > latitude) && (latitude >= -48))
        return "G";
    else if ((-48 > latitude) && (latitude >= -56))
        return "F";
    else if ((-56 > latitude) && (latitude >= -64))
        return "E";
    else if ((-64 > latitude) && (latitude >= -72))
        return "D";
    else if ((-72 > latitude) && (latitude >= -80))
        return "C";
    else
        return "Z";

}

/// <summary>
/// this function set eliptical earth radius
/// </summary>
/// <param name="name"></param>
/// <param name="a"></param>
/// <param name="eccSquared"></param>
public static  void setEllipsoid(string name ,  ref double a ,ref double eccSquared)
{
    switch (name) {
        case "Airy":
        a = 6377563;
        eccSquared = 0.00667054;
        break;
        case "Australian National":
        a = 6378160;
        eccSquared = 0.006694542;
        break;
        case "Bessel 1841":
        a = 6377397;
        eccSquared = 0.006674372;
        break;
        case "Bessel 1841 Nambia":
        a = 6377484;
        eccSquared = 0.006674372;
        break;
        case "Clarke 1866":
        a = 6378206;
        eccSquared = 0.006768658;
        break;
        case "Clarke 1880":
        a = 6378249;
        eccSquared = 0.006803511;
        break;
        case "Everest":
        a = 6377276;
        eccSquared = 0.006637847;
        break;
        case "Fischer 1960 Mercury":
        a = 6378166;
        eccSquared = 0.006693422;
        break;
        case "Fischer 1968":
        a = 6378150;
        eccSquared = 0.006693422;
        break;
        case "GRS 1967":
        a = 6378160;
        eccSquared = 0.006694605;
        break;
        case "GRS 1980":
        a = 6378137;
        eccSquared = 0.00669438;
        break;
        case "Helmert 1906":
        a = 6378200;
        eccSquared = 0.006693422;
        break;
        case "Hough":
        a = 6378270;
        eccSquared = 0.00672267;
        break;
        case "International":
        a = 6378388;
        eccSquared = 0.00672267;
        break;
        case "Krassovsky":
        a = 6378245;
        eccSquared = 0.006693422;
        break;
        case "Modified Airy":
        a = 6377340;
        eccSquared = 0.00667054;
        break;
        case "Modified Everest":
        a = 6377304;
        eccSquared = 0.006637847;
        break;
        case "Modified Fischer 1960":
        a = 6378155;
        eccSquared = 0.006693422;
        break;
        case "South American 1969":
        a = 6378160;
        eccSquared = 0.006694542;
        break;
        case "WGS 60":
        a = 6378165;
        eccSquared = 0.006693422;
        break;
        case "WGS 66":
        a = 6378145;
        eccSquared = 0.006694542;
        break;
        case "WGS 72":
        a = 6378135;
        eccSquared = 0.006694318;
        break;
        case "ED50":
        a = 6378388;
        eccSquared = 0.00672267;
            break; // International Ellipsoid
            case "WGS 84":
        case "EUREF89": // Max deviation from WGS 84 is 40 cm/km see http://ocq.dk/euref89 (in danish)
        case "ETRS89": // Same as EUREF89 
        a = 6378137;
        eccSquared = 0.00669438;
        break;
            //   new Error('No ecclipsoid data associated with unknown datum: '.name);
        }
        status = true;

    }
    public static double  toDegrees(double rad) {

        return rad / Mathf.PI * 180;
    }

    public static double toRadians( double deg) {
        return deg * Math.PI / 180;
    }
    //Convert coordenate UTM to Latitude and Longitude
    // public static void UTMtoLatitudAndLongitude(double x , double y , int zone , string h, ref double lat , ref double lon){

    
}
