using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace SimCar4._0
{
    class Car : Stuff
    {
        private float speed = 0.0f;
        private float acceleration = 0.0f;
        int gear = 1;
        bool free = true;

        private float steerAngle = GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.X;
        private int turn = 0;


        float maxSteerAngle = (float)Math.PI / 4;
        float steerSpeed = (float)Math.PI / 8;

        public float[] topSpeeds = null;
        public float[] accelerations = null;
        public float maxReverseSpeed = 0.0f;

        public float VD = 0.0f;
        public float HD = 0.0f;
        public float L = 0.0f;

        GamePadState currentState = GamePad.GetState(PlayerIndex.One);

        public Car()
        {
        }

        public void ProcessGamePad(GameTime gametime)
        {
            float throttle = GamePad.GetState(PlayerIndex.One).Triggers.Right;


            if (GamePad.GetState(PlayerIndex.One).Triggers.Left > 0.0f)
            {
                Accelerate();
            }
            else if (GamePad.GetState(PlayerIndex.One).Buttons.LeftShoulder == ButtonState.Pressed)
            {
                Brake();
            }
            else 
            {
                Free();
            }

            if (GamePad.GetState(PlayerIndex.One).DPad.Left == ButtonState.Pressed) 
            {
                Left();
            }
            else if (GamePad.GetState(PlayerIndex.One).DPad.Right == ButtonState.Pressed)
            {
                Right();
            }
            else
            {
                NoTurn();
            }
        }

        public void ProcessKeyboard(GameTime gameTime)
        {
            KeyboardState keys = Keyboard.GetState();
            if (keys.IsKeyDown(Keys.Up))
                Accelerate();
            else if (keys.IsKeyDown(Keys.Down))
                Brake();
            else
                Free();

            if (keys.IsKeyDown(Keys.Left))
                Left();
            else if (keys.IsKeyDown(Keys.Right))
                Right();
            else
                NoTurn();
        }

        public void Accelerate()
        {

            

            free = false;

            if ((gear < 7) && (speed > topSpeeds[gear]))
            {
                gear++;
            }
            if (gear == 0)
            {
                acceleration = accelerations[1];
            }
            else if (gear < 7)
                acceleration = accelerations[gear];
            else
            {
                gear = 6;
                acceleration = 0;
                speed = topSpeeds[6];
            }
        }

        public void Brake()
        {
            free = false;

            if ((gear > 0) && speed <= topSpeeds[gear - 1])
            {
                gear--;
            }
            if ((gear == 0) && (speed < -maxReverseSpeed))
            {
                acceleration = 0;
                speed = -maxReverseSpeed;
            }
            else
            {
                acceleration = -accelerations[gear] - 6.7f;
            }
        }

        public void Free()
        {
            free = true;

            if ((gear > 0) && (speed < topSpeeds[gear - 1]))
            {
                gear--;
            }

            acceleration = -accelerations[gear];
        }

        public void Left()
        {
            turn = -1;
        }

        public void Right()
        {
            turn = 1;
        }

        public void NoTurn()
        {
            turn = 0;
        }

        public void Update(GameTime gameTime)
        {
            float time = ((float)gameTime.ElapsedGameTime.Milliseconds) / 1000;
            if (turn == 0)
            {
                float newAngle = steerAngle;
                if (steerAngle < 0.0f)
                {
                    newAngle = steerAngle + time * steerSpeed;
                }
                else if (steerAngle > 0.0f)
                {
                    newAngle = steerAngle - time * steerSpeed;
                }
                if (newAngle * steerAngle < 0.0f)
                {
                    steerAngle = 0.0f;
                }
                else
                    steerAngle = newAngle;
            }
            else
            {
                if (turn == -1)
                {
                    float newAngle = steerAngle - time * steerSpeed;
                    if (newAngle < -maxSteerAngle)
                    {
                        steerAngle = -maxSteerAngle;
                    }
                    else
                    {
                        steerAngle = newAngle;
                    }
                }
                else
                {
                    float newAngle = steerAngle + time * steerSpeed;
                    if (newAngle > maxSteerAngle)
                    {
                        steerAngle = maxSteerAngle;
                    }
                    else
                    {
                        steerAngle = newAngle;
                    }
                }
            }

            if (steerAngle != 0.0f)
            {
                float x = (VD / Math.Abs((float)Math.Tan(steerAngle)) + HD / 2);
                float r = (float)Math.Sqrt(x * x + L * L);
                float theta = speed * time / r;

                if (steerAngle < 0.0f)
                    theta = -theta;

                rotation = rotation * Quaternion.CreateFromAxisAngle(new Vector3(0, -1, 0), theta);
            }

            float newSpeed = speed + acceleration * time;

            // If neither accelerator not brake is pressed car will come to stop as speed decreases
            // not start to move in opposite direction
            if ((free == true) && (newSpeed * speed <= 0.0f))
            {
                gear = 1;
                acceleration = accelerations[gear];
                speed = 0.0f;
            }
            else
                speed = newSpeed;

            float dist = speed * time;
            Vector3 addVector = Vector3.Transform(new Vector3(0, 0, -1), rotation);
            position += addVector * dist;
        }

        public static SpriteFont font;

        public void DrawInfo(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(font, steerAngle.ToString(), Vector2.Zero, Color.White);
        }
    }
}
