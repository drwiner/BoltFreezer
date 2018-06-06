using Cinematography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CameraNamespace
{
    [Serializable]
    public class CamSchema
    {

        public FramingType scale = FramingType.None;

        public string targetLocation;

        public int targetOrientation = -1;

        public int hangle = -1;

        public int vangle = -1;

        public CamSchema(FramingType _scale, string _tLoc, int _tOrient, int _hangle, int _vangle)
        {
            scale = _scale;
            targetLocation = _tLoc;
            targetOrientation = _tOrient;
            hangle = _hangle;
            vangle = _vangle;
        }

        public CamSchema Duplicate()
        {
            return new CamSchema(scale, targetLocation, targetOrientation, hangle, vangle);
        }

        public override string ToString()
        {
            return string.Format("Shot({0}.{1}.{2}.{3}.{4}", scale, targetLocation, targetOrientation, hangle, vangle);
        }

        public bool IsConsistent(CamSchema cas)
        {
            if (scale != FramingType.None)
            {
                if (scale != cas.scale)
                {
                    //Debug.Log("not same scale");
                    return false;
                }
            }

            if (targetLocation != "" && targetLocation != null)
            {
                if (targetLocation != cas.targetLocation)
                {
                    //Debug.Log("not same location");
                    return false;
                }
            }

            if (targetOrientation != -1)
            {
                if (targetOrientation != cas.targetOrientation)
                {
                    //Debug.Log("not same target Orient");
                    return false;
                }
            }

            if (hangle != -1)
            {
                if (hangle != cas.hangle)
                {
                    //Debug.Log("not same hangle");
                    return false;
                }
            }

            if (vangle != -1)
            {
                if (vangle != cas.vangle)
                {
                    //Debug.Log("not same vangle");
                    return false;
                }
            }

            return true;
        }

        public CamSchema Clone()
        {
            return new CamSchema(scale, targetLocation, targetOrientation, hangle, vangle);
        }
    }
}
