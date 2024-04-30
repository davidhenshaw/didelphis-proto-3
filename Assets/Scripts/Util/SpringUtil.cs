///Much of the code flagrantly stolen from https://www.ryanjuckett.com/damped-springs/, thank you for your service o7

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SpringUtil
{
    public static void CalcDampedSimpleHarmonicMotion(ref float value, ref float velocity, float goalValue, float deltaTime, float frequency, float damping)
    {
        float xCoef = 1;
        float dxCoef = 0;
        float vCoef = 0;
        float dvCoef = 1;

        if(damping < 0) { damping = 0; }
        if(frequency < 0) { frequency = 0; }

        if (frequency < Mathf.Epsilon)
        {
            float oldPoss = value - goalValue; // update in equilibrium relative space
            float oldVell = velocity;

            value = oldPoss * xCoef + oldVell * dxCoef + goalValue;
            velocity = oldPoss * vCoef + oldVell * dvCoef;

            return;
        }

        if ( damping > 1 + Mathf.Epsilon )
        {
            //over-damped
            float za = -frequency * damping;
            float zb = frequency * Mathf.Sqrt(damping * damping - 1.0f);
            float z1 = za - zb;
            float z2 = za + zb;

            float e1 = Mathf.Exp(z1 * deltaTime);
            float e2 = Mathf.Exp(z2 * deltaTime);

            float invTwoZb = 1.0f / (2.0f * zb); // = 1 / (z2 - z1)

            float e1_Over_TwoZb = e1 * invTwoZb;
            float e2_Over_TwoZb = e2 * invTwoZb;

            float z1e1_Over_TwoZb = z1 * e1_Over_TwoZb;
            float z2e2_Over_TwoZb = z2 * e2_Over_TwoZb;

            xCoef = e1_Over_TwoZb * z2 - z2e2_Over_TwoZb + e2;
            dxCoef = -e1_Over_TwoZb + e2_Over_TwoZb;

            vCoef = (z1e1_Over_TwoZb - z2e2_Over_TwoZb + e2) * z2;
            dvCoef = -z1e1_Over_TwoZb + z2e2_Over_TwoZb;
        }
        else if(damping < 1 - Mathf.Epsilon )
        {
            // under-damped
            float omegaZeta = frequency * damping;
            float alpha = frequency * Mathf.Sqrt(1.0f - damping * damping);

            float expTerm = Mathf.Exp(-omegaZeta * deltaTime);
            float cosTerm = Mathf.Cos(alpha * deltaTime);
            float sinTerm = Mathf.Sin(alpha * deltaTime);

            float invAlpha = 1.0f / alpha;

            float expSin = expTerm * sinTerm;
            float expCos = expTerm * cosTerm;
            float expOmegaZetaSin_Over_Alpha = expTerm * omegaZeta * sinTerm * invAlpha;

            xCoef = expCos + expOmegaZetaSin_Over_Alpha;
            dxCoef = expSin * invAlpha;

            vCoef = -expSin * alpha - omegaZeta * expOmegaZetaSin_Over_Alpha;
            dvCoef = expCos - expOmegaZetaSin_Over_Alpha;
        }
        else
        {
            // critically damped
            float expTerm = Mathf.Exp(frequency * deltaTime);
            float timeExp = deltaTime * expTerm;
            float timeExpFreq = timeExp * frequency;

            xCoef = timeExpFreq + expTerm;
            dxCoef = timeExp;

            vCoef = -frequency * timeExpFreq;
            dvCoef = -timeExpFreq + expTerm;
        }


        float oldPos = value - goalValue; // update in equilibrium relative space
        float oldVel = velocity; 

        value = oldPos * xCoef + oldVel * dxCoef + goalValue;
        velocity = oldPos * vCoef  + oldVel * dvCoef;
    }

}
