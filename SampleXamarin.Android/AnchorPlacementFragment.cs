﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using Google.AR.Core;
using Google.AR.Sceneform.UX;
using System;
using System.Threading;
using static Android.Views.View;
using static Android.Widget.RadioGroup;
using static Google.AR.Sceneform.UX.BaseArFragment;
using Color = Android.Graphics.Color;

namespace SampleXamarin
{
    internal class AnchorPlacementFragment
        : Fragment, IOnTapArPlaneListener, IOnClickListener, IOnCheckedChangeListener
    {
        private ArFragment arFragment;
        private AnchorVisual visual;

        private TextView hintText;
        private Button syncTelemetryButton;
        private RadioGroup shapeSelection;
        private LinearLayout selectLayout;
        private Timer timer;
        private AnchorVisual.NamedShape SelectedShape
        {
            get
            {
                int selectedId = shapeSelection.CheckedRadioButtonId;
                if (selectedId == Resource.Id.sphere_shape)
                {
                    return AnchorVisual.NamedShape.Sphere;
                }
                else if (selectedId == Resource.Id.cylinder_shape)
                {
                    return AnchorVisual.NamedShape.Cylinder;
                }
                else if (selectedId == Resource.Id.cube_shape)
                {
                    return AnchorVisual.NamedShape.Cube;
                }

                throw new InvalidOperationException("Invalid selected shape");
            }
        }

        public delegate void AnchorPlacementListener(AnchorVisual visual);

        public AnchorPlacementListener OnAnchorPlaced { private get; set; }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.coarse_reloc_anchor_placement, container, false);
        }

        public override void OnAttach(Context context)
        {
            base.OnAttach(context);

            FragmentActivity activity = (FragmentActivity)context;
            arFragment = (ArFragment)activity.SupportFragmentManager.FindFragmentById(Resource.Id.ar_fragment);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            hintText = view.FindViewById<TextView>(Resource.Id.hint_text);
            syncTelemetryButton = view.FindViewById<Button>(Resource.Id.confirm_placement);
            shapeSelection = view.FindViewById<RadioGroup>(Resource.Id.shape_selection);
            selectLayout = view.FindViewById<LinearLayout>(Resource.Id.co_select_layout);
        }

        public override void OnStart()
        {
            base.OnStart();

            syncTelemetryButton.Enabled = false;
            selectLayout.Visibility = ViewStates.Invisible;
            arFragment.SetOnTapArPlaneListener(this);
            syncTelemetryButton.SetOnClickListener(this);
            shapeSelection.SetOnCheckedChangeListener(this);
        }

        public override void OnStop()
        {
            arFragment.SetOnTapArPlaneListener(null);

            if (visual != null)
            {
                visual.ModelLoaded -= Visual_ModelLoaded;
                visual.Destroy();
                visual = null;

            }
            if(timer != null)
            {
                timer.Dispose();
            }
           
            base.OnStop();
        }

        public void OnTapPlane(HitResult hitResult, Plane plane, MotionEvent motionEvent)
        {
            if (visual != null)
            {
                return;
            }

            Anchor localAnchor = hitResult.CreateAnchor();
            visual = new AnchorVisual(arFragment, localAnchor);
            visual.IsMovable = true;
            visual.Shape = SelectedShape;
            visual.SetColor(arFragment.Context, Color.Yellow);
            visual.AddToScene(arFragment);
            visual.ModelLoaded += Visual_ModelLoaded;
            hintText.SetText(Resource.String.hint_adjust_anchor);

        }

        private void Visual_ModelLoaded(object sender, bool e)
        {
            syncTelemetryButton.Enabled = true;
        }

        void IOnCheckedChangeListener.OnCheckedChanged(RadioGroup radioGroup, int selectedId)
        {
            if (visual == null)
            {
                return;
            }

            visual.Shape = SelectedShape;
        }

        public void OnClick(View view)
        {
            if (visual != null)
            {
                syncTelemetryButton.Enabled = false;
                visual.Fetch_LoadTelemetry();
                syncTelemetryButton.Enabled = true;
                timer = new Timer(TimerCallback, null, TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(20));
                //AnchorVisual placedAnchor = visual;
                //visual = null;
                //placedAnchor.IsMovable = false;
                //OnAnchorPlaced?.Invoke(placedAnchor);
            }
        }

        private void TimerCallback(object state)
        {
            Console.WriteLine($"Timer callback executed at: {DateTime.Now}");
            if (visual != null)
            {
                visual.Fetch_LoadTelemetry();
            }
            
        }
    }
}
