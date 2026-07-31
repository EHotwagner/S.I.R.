namespace SIR.Client

type SidebarSide =
    | Left
    | Right

type TacticalPanelDefinition =
    { Id: string
      Label: string
      DefaultSide: SidebarSide
      DefaultOrder: int
      DefaultVisible: bool
      DefaultCollapsed: bool }

type PanelPlacement =
    { PanelId: string
      Side: SidebarSide
      Order: int
      Visible: bool
      Collapsed: bool }

type SidebarLayout =
    { Width: int
      DrawerOpen: bool }

type BottomPanelLayout =
    { Visible: bool
      Height: int
      CollapsedInEditor: bool
      CollapsedOutsideEditor: bool }

type TacticalLayoutProfile =
    { SchemaVersion: int
      Placements: PanelPlacement list
      LeftSidebar: SidebarLayout
      RightSidebar: SidebarLayout
      BottomPanel: BottomPanelLayout }

type TacticalLayoutDiagnostic =
    | UnknownPanel of string
    | DuplicatePanel of string
    | MalformedLayoutProfile of string
    | UnsupportedLayoutSchema of int
    | InvalidLayoutDimension of name: string * value: int

[<RequireQualifiedAccess>]
module TacticalWorkspaceLayout =
    [<Literal>]
    val SchemaVersion: int = 1

    val panelRegistry: TacticalPanelDefinition list
    val fieldFocus: TacticalLayoutProfile
    val panelsOn: SidebarSide -> TacticalLayoutProfile -> PanelPlacement list
    val bottomVisible: TacticalLayoutProfile -> bool
    val bottomCollapsed: TacticalModality -> TacticalLayoutProfile -> bool
    val togglePanelVisibility: string -> TacticalLayoutProfile -> TacticalLayoutProfile
    val togglePanelCollapsed: string -> TacticalLayoutProfile -> TacticalLayoutProfile
    val movePanel: string -> SidebarSide -> TacticalLayoutProfile -> TacticalLayoutProfile
    val reorderPanel: string -> int -> TacticalLayoutProfile -> TacticalLayoutProfile
    val toggleDrawer: SidebarSide -> TacticalLayoutProfile -> TacticalLayoutProfile
    val toggleBottomPanelVisibility: TacticalLayoutProfile -> TacticalLayoutProfile
    val toggleBottomPanel: TacticalModality -> TacticalLayoutProfile -> TacticalLayoutProfile
    val resizeBottomPanel: int -> TacticalLayoutProfile -> TacticalLayoutProfile
    val reset: TacticalLayoutProfile -> TacticalLayoutProfile
    val exportProfile: TacticalLayoutProfile -> string
    val importProfile: string -> Result<TacticalLayoutProfile, TacticalLayoutDiagnostic list>
