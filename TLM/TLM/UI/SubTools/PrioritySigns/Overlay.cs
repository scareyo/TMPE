namespace TrafficManager.UI.SubTools.PrioritySigns {
    using ColossalFramework;
    using TrafficManager.API.Manager;
    using TrafficManager.Manager.Impl;
    using TrafficManager.State;
    using TrafficManager.UI.Textures;
    using TrafficManager.Util;
    using UnityEngine;

    /// <summary>
    /// Class handles rendering of priority signs overlay.
    /// Create one and set its fields before calling DrawSignHandles
    /// </summary>
    public struct Overlay {
        private const float JUNCTION_RESTRICTIONS_SIGN_SIZE = 80f;

        private readonly TrafficManagerTool mainTool_;
        private readonly bool debug_;
        private readonly bool handleClick_;

        public bool ViewOnly;

        /// <summary>Initializes a new instance of the <see cref="Overlay"/> struct for rendering.</summary>
        /// <param name="mainTool">Parent <see cref="TrafficManagerTool"/>.</param>
        /// <param name="debug">Is debug rendering on.</param>
        /// <param name="handleClick">Whether clicks are to be handled.</param>
        public Overlay(TrafficManagerTool mainTool,
                       bool debug,
                       bool handleClick) {
            mainTool_ = mainTool;
            debug_ = debug;
            handleClick_ = handleClick;
            ViewOnly = true;
        }

        public bool DrawSignHandles(ushort nodeId,
                                    ref NetNode node,
                                    ref Vector3 camPos,
                                    out bool stateUpdated) {
            bool hovered = false;
            stateUpdated = false;

            // Quit now if:
            //   * view only,
            //   * and no permanent overlay enabled,
            //   * and is not Prio Signs tool
            if (this.ViewOnly &&
                !(Options.junctionRestrictionsOverlay ||
                  MassEditOverlay.IsActive) &&
                this.mainTool_.GetToolMode() != ToolMode.JunctionRestrictions) {
                return false;
            }

            // NetManager netManager = Singleton<NetManager>.instance;
            Color guiColor = GUI.color;
            Vector3 nodePos = Singleton<NetManager>.instance.m_nodes.m_buffer[nodeId].m_position;
            IExtSegmentEndManager segEndMan = Constants.ManagerFactory.ExtSegmentEndManager;

            for (int i = 0; i < 8; ++i) {
                ushort segmentId = node.GetSegment(i);

                if (segmentId == 0) {
                    continue;
                }

                bool isStartNode =
                    (bool)Constants.ServiceFactory.NetService.IsStartNode(segmentId, nodeId);

                bool incoming = segEndMan
                                .ExtSegmentEnds[segEndMan.GetIndex(segmentId, isStartNode)]
                                .incoming;

                int numSignsPerRow = incoming ? 2 : 1;

                NetInfo segmentInfo = Singleton<NetManager>
                                      .instance
                                      .m_segments
                                      .m_buffer[segmentId]
                                      .Info;

                ItemClass connectionClass = segmentInfo.GetConnectionClass();

                if (connectionClass.m_service != ItemClass.Service.Road) {
                    continue; // only for road junctions
                }

                //------------------------------------
                // Draw all junction restriction signs
                // Determine direction from node center towards each segment center and use that
                // as axis Y, and then dot product gives "horizontal" axis X
                //------------------------------------
                Vector3 segmentCenterPos = Singleton<NetManager>
                                           .instance
                                           .m_segments
                                           .m_buffer[segmentId]
                                           .m_bounds
                                           .center;

                // Unit vector towards the segment center
                Vector3 yu = (segmentCenterPos - nodePos).normalized;
                // Unit vector perpendicular to the vector towards the segment center
                Vector3 xu = Vector3.Cross(yu, new Vector3(0f, 1f, 0f)).normalized;

                float f = this.ViewOnly ? 6f : 7f; // reserved sign size in game coordinates

                Vector3 centerStart = nodePos + (yu * (this.ViewOnly ? 5f : 14f));
                Vector3 zero = centerStart - (0.5f * (numSignsPerRow - 1) * f * xu); // "top left"
                if (this.ViewOnly) {
                    if (Constants.ServiceFactory.SimulationService.TrafficDrivesOnLeft) {
                        zero -= xu * 8f;
                    } else {
                        zero += xu * 8f;
                    }
                }

                bool signHovered;
                int x = 0;
                int y = 0;
                bool hasSignInPrevRow = false;
                IJunctionRestrictionsManager junctionRManager = Constants.ManagerFactory.JunctionRestrictionsManager;

                // draw "lane-changing when going straight allowed" sign at (0; 0)
                bool allowed =
                    junctionRManager.IsLaneChangingAllowedWhenGoingStraight(
                        segmentId: segmentId,
                        startNode: isStartNode);

                bool configurable =
                    junctionRManager.IsLaneChangingAllowedWhenGoingStraightConfigurable(
                        segmentId: segmentId,
                        startNode: isStartNode,
                        node: ref node);

                if (this.debug_
                    || (configurable
                        && (!this.ViewOnly
                            || (allowed != junctionRManager
                                           .GetDefaultLaneChangingAllowedWhenGoingStraight(
                                               segmentId: segmentId,
                                               startNode: isStartNode,
                                               node: ref node)))))
                {
                    this.DrawSign(
                        small: !configurable,
                        camPos: ref camPos,
                        xu: ref xu,
                        yu: ref yu,
                        f: f,
                        zero: ref zero,
                        x: x,
                        y: y,
                        guiColor: guiColor,
                        signTexture: allowed
                                         ? JunctionRestrictions.LaneChangeAllowed
                                         : JunctionRestrictions.LaneChangeForbidden,
                        hoveredHandle: out signHovered);

                    if (signHovered && this.handleClick_) {
                        hovered = true;
                        if (this.mainTool_.CheckClicked()) {
                            junctionRManager.ToggleLaneChangingAllowedWhenGoingStraight(
                                segmentId: segmentId,
                                startNode: isStartNode);
                            stateUpdated = true;
                        }
                    }

                    ++x;
                    hasSignInPrevRow = true;
                }

                // draw "u-turns allowed" sign at (1; 0)
                allowed = junctionRManager.IsUturnAllowed(segmentId, isStartNode);
                configurable = junctionRManager.IsUturnAllowedConfigurable(
                    segmentId: segmentId,
                    startNode: isStartNode,
                    node: ref node);

                if (this.debug_
                    || (configurable
                        && (!this.ViewOnly
                            || (allowed != junctionRManager.GetDefaultUturnAllowed(
                                    segmentId: segmentId,
                                    startNode: isStartNode,
                                    node: ref node)))))
                {
                    this.DrawSign(
                        small: !configurable,
                        camPos: ref camPos,
                        xu: ref xu,
                        yu: ref yu,
                        f: f,
                        zero: ref zero,
                        x: x,
                        y: y,
                        guiColor: guiColor,
                        signTexture: allowed
                                         ? JunctionRestrictions.UturnAllowed
                                         : JunctionRestrictions.UturnForbidden,
                        hoveredHandle: out signHovered);

                    if (signHovered && this.handleClick_) {
                        hovered = true;

                        if (this.mainTool_.CheckClicked()) {
                            if (!junctionRManager.ToggleUturnAllowed(
                                    segmentId,
                                    isStartNode)) {
                                // TODO MainTool.ShowTooltip(Translation.GetString("..."), Singleton<NetManager>.instance.m_nodes.m_buffer[nodeId].m_position);
                            } else {
                                stateUpdated = true;
                            }
                        }
                    }

                    x++;
                    hasSignInPrevRow = true;
                }

                x = 0;
                if (hasSignInPrevRow) {
                    ++y;
                    hasSignInPrevRow = false;
                }

                // draw "entering blocked junctions allowed" sign at (0; 1)
                allowed = junctionRManager.IsEnteringBlockedJunctionAllowed(
                    segmentId: segmentId,
                    startNode: isStartNode);
                configurable = junctionRManager.IsEnteringBlockedJunctionAllowedConfigurable(
                    segmentId: segmentId,
                    startNode: isStartNode,
                    node: ref node);

                if (this.debug_
                    || (configurable
                        && (!this.ViewOnly
                            || (allowed != junctionRManager
                                                    .GetDefaultEnteringBlockedJunctionAllowed(
                                                        segmentId,
                                                        isStartNode,
                                                        ref node))))) {
                    this.DrawSign(
                        small: !configurable,
                        camPos: ref camPos,
                        xu: ref xu,
                        yu: ref yu,
                        f: f,
                        zero: ref zero,
                        x: x,
                        y: y,
                        guiColor: guiColor,
                        signTexture: allowed
                                         ? JunctionRestrictions.EnterBlockedJunctionAllowed
                                         : JunctionRestrictions.EnterBlockedJunctionForbidden,
                        hoveredHandle: out signHovered);

                    if (signHovered && this.handleClick_) {
                        hovered = true;

                        if (this.mainTool_.CheckClicked()) {
                            junctionRManager.ToggleEnteringBlockedJunctionAllowed(
                                segmentId: segmentId,
                                startNode: isStartNode);
                            stateUpdated = true;
                        }
                    }

                    ++x;
                    hasSignInPrevRow = true;
                }

                // draw "pedestrian crossing allowed" sign at (1; 1)
                allowed = junctionRManager.IsPedestrianCrossingAllowed(
                    segmentId: segmentId,
                    startNode: isStartNode);
                configurable = junctionRManager.IsPedestrianCrossingAllowedConfigurable(
                    segmentId: segmentId,
                    startNode: isStartNode,
                    node: ref node);

                if (this.debug_
                    || (configurable
                        && (!this.ViewOnly || !allowed))) {
                    this.DrawSign(
                        small: !configurable,
                        camPos: ref camPos,
                        xu: ref xu,
                        yu: ref yu,
                        f: f,
                        zero: ref zero,
                        x: x,
                        y: y,
                        guiColor: guiColor,
                        signTexture: allowed
                                         ? JunctionRestrictions.PedestrianCrossingAllowed
                                         : JunctionRestrictions.PedestrianCrossingForbidden,
                        hoveredHandle: out signHovered);

                    if (signHovered && this.handleClick_) {
                        hovered = true;

                        if (this.mainTool_.CheckClicked()) {
                            junctionRManager.TogglePedestrianCrossingAllowed(
                                segmentId,
                                isStartNode);
                            stateUpdated = true;
                        }
                    }

                    x++;
                    hasSignInPrevRow = true;
                }

                x = 0;

                if (hasSignInPrevRow) {
                    ++y;
                    hasSignInPrevRow = false;
                }

                if (!Options.turnOnRedEnabled) {
                    continue;
                }

                //--------------------------------
                // TURN ON RED ENABLED
                //--------------------------------
                bool leftSideTraffic = Constants.ServiceFactory.SimulationService.TrafficDrivesOnLeft;

                // draw "turn-left-on-red allowed" sign at (2; 0)
                allowed = junctionRManager.IsTurnOnRedAllowed(
                    near: leftSideTraffic,
                    segmentId: segmentId,
                    startNode: isStartNode);
                configurable = junctionRManager.IsTurnOnRedAllowedConfigurable(
                    near: leftSideTraffic,
                    segmentId: segmentId,
                    startNode: isStartNode,
                    node: ref node);

                if (this.debug_
                    || (configurable
                        && (!this.ViewOnly
                            || (allowed != junctionRManager.GetDefaultTurnOnRedAllowed(
                                    near: leftSideTraffic,
                                    segmentId: segmentId,
                                    startNode: isStartNode,
                                    node: ref node)))))
                {
                    this.DrawSign(
                        small: !configurable,
                        camPos: ref camPos,
                        xu: ref xu,
                        yu: ref yu,
                        f: f,
                        zero: ref zero,
                        x: x,
                        y: y,
                        guiColor: guiColor,
                        signTexture: allowed
                                         ? JunctionRestrictions.LeftOnRedAllowed
                                         : JunctionRestrictions.LeftOnRedForbidden,
                        hoveredHandle: out signHovered);

                    if (signHovered && this.handleClick_) {
                        hovered = true;

                        if (this.mainTool_.CheckClicked()) {
                            junctionRManager.ToggleTurnOnRedAllowed(
                                near: leftSideTraffic,
                                segmentId: segmentId,
                                startNode: isStartNode);
                            stateUpdated = true;
                        }
                    }

                    hasSignInPrevRow = true;
                }

                x++;

                // draw "turn-right-on-red allowed" sign at (2; 1)
                allowed = junctionRManager.IsTurnOnRedAllowed(
                    near: !leftSideTraffic,
                    segmentId: segmentId,
                    startNode: isStartNode);
                configurable = junctionRManager.IsTurnOnRedAllowedConfigurable(
                    near: !leftSideTraffic,
                    segmentId: segmentId,
                    startNode: isStartNode,
                    node: ref node);

                if (this.debug_
                    || (configurable
                        && (!this.ViewOnly
                            || (allowed != junctionRManager.GetDefaultTurnOnRedAllowed(
                                    near: !leftSideTraffic,
                                    segmentId: segmentId,
                                    startNode: isStartNode,
                                    node: ref node)))))
                {
                    this.DrawSign(
                        small: !configurable,
                        camPos: ref camPos,
                        xu: ref xu,
                        yu: ref yu,
                        f: f,
                        zero: ref zero,
                        x: x,
                        y: y,
                        guiColor: guiColor,
                        signTexture: allowed
                                         ? JunctionRestrictions.RightOnRedAllowed
                                         : JunctionRestrictions.RightOnRedForbidden,
                        hoveredHandle: out signHovered);

                    if (signHovered && this.handleClick_) {
                        hovered = true;

                        if (this.mainTool_.CheckClicked()) {
                            junctionRManager.ToggleTurnOnRedAllowed(
                                near: !leftSideTraffic,
                                segmentId: segmentId,
                                startNode: isStartNode);
                            stateUpdated = true;
                        }
                    }

                    hasSignInPrevRow = true;
                }
            }

            guiColor.a = 1f;
            GUI.color = guiColor;

            return hovered;
        }

        private void DrawSign(bool small,
                              ref Vector3 camPos,
                              ref Vector3 xu,
                              ref Vector3 yu,
                              float f,
                              ref Vector3 zero,
                              int x,
                              int y,
                              Color guiColor,
                              Texture2D signTexture,
                              out bool hoveredHandle) {
            Vector3 signCenter = zero + (f * x * xu) + (f * y * yu); // in game coordinates
            bool visible = GeometryUtil.WorldToScreenPoint(signCenter, out Vector3 signScreenPos);

            if (!visible) {
                hoveredHandle = false;
                return;
            }

            Vector3 diff = signCenter - camPos;
            float zoom = 100.0f * this.mainTool_.GetBaseZoom() / diff.magnitude;
            float size = (small ? 0.75f : 1f)
                         * (this.ViewOnly ? 0.8f : 1f)
                         * JUNCTION_RESTRICTIONS_SIGN_SIZE * zoom;

            var boundingBox = new Rect(
                x: signScreenPos.x - (size / 2),
                y: signScreenPos.y - (size / 2),
                width: size,
                height: size);
            hoveredHandle = !this.ViewOnly && TrafficManagerTool.IsMouseOver(boundingBox);
            guiColor.a = TrafficManagerTool.GetHandleAlpha(hoveredHandle);

            GUI.color = guiColor;
            GUI.DrawTexture(boundingBox, signTexture);
        }
    } // end class
}