module SIR.Client.Web.EnvironmentFeatureContract

open SIR.Client

type Callbacks =
    { ParcelChanged: TacticalParcelEditor.TacticalParcelEditorAction -> unit
      EnterSimulation: unit -> unit
      ImportTextChanged: string -> unit
      ImportDocument: unit -> unit
      ExportDocument: unit -> unit
      SimulatorChanged: SimulatorAction -> unit
      ResetSimulator: unit -> unit }
