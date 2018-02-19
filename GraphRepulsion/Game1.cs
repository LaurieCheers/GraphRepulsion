using LRCEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace GraphRepulsion
{
    class Node
    {
        public Vector2 position;
        public Color color;
        List<Bond> bonds = new List<Bond>();

        public Node(Vector2 position, Color color)
        {
            this.position = position;
            this.color = color;
        }

        public void Add(Bond bond)
        {
            bonds.Add(bond);
        }
    }

    class Bond
    {
        public Node from;
        public Node to;
        float length;
        public Color color;

        public Bond(Node from, Node to)
        {
            this.from = from;
            this.to = to;
            this.length = (from.position - to.position).Length();
        }

        public Bond(Node from, Node to, float length, Color color)
        {
            this.from = from;
            this.to = to;
            this.length = length;
            this.color = color;
        }

        public void UpdateForces()
        {
            Vector2 offset = (from.position - to.position);
            float distance = offset.Length();
            float delta = (distance - length);
            Vector2 force = delta/**delta*(delta>0?1:-1)*/ * 0.1f * offset / distance;
            from.position -= force;
            to.position +=force;
        }
    }

    class NodeSystem
    {
        public List<Node> nodes = new List<Node>();
        List<Bond> bonds = new List<Bond>();
        public Vector2 drawOffset;

        public void AddNode(Node n)
        {
            nodes.Add(n);
        }

        public void AddBond(Node from, Node to)
        {
            Bond newBond = new Bond(from, to);
            from.Add(newBond);
            to.Add(newBond);
            bonds.Add(newBond);
        }

        public void AddBond(Node from, Node to, float length, Color color)
        {
            Bond newBond = new Bond(from, to, length, color);
            from.Add(newBond);
            to.Add(newBond);
            bonds.Add(newBond);
        }
        
        public void UpdateForces()
        {
            foreach(Bond bond in bonds)
            {
                bond.UpdateForces();
            }
        }

        public Vector2 CenterOfGravity()
        {
            Vector2 center = Vector2.Zero;
            float averager = 1.0f / nodes.Count;
            foreach (Node node in nodes)
            {
                center += node.position * averager;
            }
            return center;
        }

        public void RepelFromCenterOfGravity(float strength)
        {
            Vector2 center = CenterOfGravity();
            foreach (Node node in nodes)
            {
                Vector2 offset = (node.position - center);
                float modStrength = (float)Math.Sqrt(offset.Length());
                offset.Normalize();
                //offset.Normalize();
                node.position += offset * strength * modStrength;
            }
        }

        public void RepelNodes(float maxRange, float strength)
        {
            float maxRangeSqr = maxRange * maxRange;
            Vector2[] forces = new Vector2[nodes.Count];
            for (int AIdx = 0; AIdx < nodes.Count; ++AIdx)
            {
                Node a = nodes[AIdx];
                for (int BIdx = AIdx + 1; BIdx < nodes.Count; ++BIdx)
                {
                    Node b = nodes[BIdx];

                    Vector2 offset = (a.position - b.position);
                    float offsLengthSqr = offset.LengthSquared();
                    if(offsLengthSqr < maxRangeSqr)
                    {
                        float offsetLength = offset.Length();
                        Vector2 direction = offset/offsetLength;
                        float localStrength = MathHelper.Lerp(strength, 0, offsetLength / maxRange);
                        Vector2 force = direction * localStrength;
                        forces[AIdx] += force;
                        forces[BIdx] -= force;
                    }
                }
            }
            for (int AIdx = 0; AIdx < nodes.Count; ++AIdx)
            {
                nodes[AIdx].position += forces[AIdx];
            }
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D nodeSprite)
        {
            foreach (Bond bond in bonds)
            {
                spriteBatch.DrawLine(Game1.whiteTexture, bond.from.position+ drawOffset, bond.to.position+ drawOffset, 4, bond.color);
            }

            foreach (Node node in nodes)
            {
                spriteBatch.Draw(nodeSprite, node.position + drawOffset - new Vector2(8,8), node.color);
            }
        }
    }

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        NodeSystem system = new NodeSystem();
        Texture2D nodeSprite;
        System.Random random = new System.Random();
        Vector2 mouseDownPos;
        bool mouseWasDown = false;
        public static Texture2D whiteTexture;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            Node a = new Node(new Vector2(50, 50), Color.Red);
            Node b = new Node(new Vector2(150, 50), Color.Blue);
            system.AddNode(a);
            system.AddNode(b);
            /*            system.AddNode(c);
                        system.AddBond(a, b, 50);
                        system.AddBond(b, c, 80);
                        system.AddBond(c, a, 50);
                        Node lastNodeC = MakeChain(c, 10, 20.0f);
                        Node v = MakeChain(c, 100, 20.0f);
                        Node w = MakeChain(c, 100, 20.0f);
                        system.AddBond(v, w, 100);
                        //system.AddBond(lastNodeC, b, 20.0f);
            */
            Node lastNode = a;
            for (int Idx = 0; Idx < 100; ++Idx)
            {
                Node newNode = new Node(new Vector2((float)Math.Sin(Idx*0.1f)*Idx*5.0f, (float)Math.Cos(Idx*0.1f)*Idx*5.0f), Color.Yellow);
                system.AddNode(newNode);
                system.AddBond(lastNode, newNode, 5, Color.Gray);
                lastNode = newNode;
            }

//            Node lastNodeB = MakeChain(b, 100, 5.0f);
            system.AddBond(lastNode, b, 5.0f, Color.White);
        }

        Node MakeChain(Node x, int length, float distances)
        {
            Node lastNode = x;
            for (int Idx = 0; Idx < length; ++Idx)
            {
                Node newNode = new Node(new Vector2(random.Next(50, 100), random.Next(50, 100)), Color.Yellow);
                system.AddNode(newNode);
                system.AddBond(lastNode, newNode, distances, Color.White);
                lastNode = newNode;
            }
            return lastNode;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            nodeSprite = Content.Load<Texture2D>("nodeSprite");
            whiteTexture = Content.Load<Texture2D>("white");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        InputState input = new InputState();
        Node lastClickedNode = null;
        float selectRadius = 10;

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            input.Update();

            MouseState mouse = Mouse.GetState();
            if (input.mouseLeft.justPressed)
            {
                foreach(Node n in system.nodes)
                {
                    if((input.MousePos - system.drawOffset - n.position).LengthSquared() < selectRadius*selectRadius)
                    {
                        if (lastClickedNode == null || lastClickedNode == n)
                        {
                            lastClickedNode = n;
                        }
                        else
                        {
                            system.AddBond(lastClickedNode, n, 5, Color.White);
                            lastClickedNode = null;
                        }
                        break;
                    }
                }
                mouseDownPos = mouse.Position.ToVector2() - system.drawOffset;


            }
            else if(input.mouseLeft.isDown)
            {
                system.drawOffset = mouse.Position.ToVector2() - mouseDownPos;
            }

            if(input.mouseRight.justPressed)
            {
                int random1 = random.Next(system.nodes.Count - 3);
                int random2 = random.Next(random1+3, system.nodes.Count);
                Node node1 = system.nodes[random1];
                Node node2 = system.nodes[random2];
                Color color = new Color(random.Next(255), random.Next(255), random.Next(128));
                node1.color = color;
                node2.color = color;
                system.AddBond(node1, node2, 5.0f, color);
            }
            // TODO: Add your update logic here
            system.UpdateForces();
            system.RepelFromCenterOfGravity(0.01f);
            system.RepelNodes(150, 1.0f);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            system.Draw(spriteBatch, nodeSprite);
          //  spriteBatch.Draw(nodeSprite, system.CenterOfGravity() + system.drawOffset, Color.Black);
//            spriteBatch.Draw(nodeSprite, new Vector2(50, 50), Color.Red);
            spriteBatch.End();
            
            base.Draw(gameTime);
        }
    }
}
