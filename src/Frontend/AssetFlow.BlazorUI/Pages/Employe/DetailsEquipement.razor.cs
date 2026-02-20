// ============================================================
// Pages/Employe/DetailsEquipement.razor.cs
//
// Code-behind de la page détail équipement.
// Utilise exactement EmployeService + EquipementAffecteDto + SignalerIncidentDto
// tels que définis dans Services/EmployeService.cs du projet réel.
// ============================================================

using AssetFlow.BlazorUI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text;

namespace AssetFlow.BlazorUI.Pages.Employe
{
    public partial class DetailsEquipement
    {
        // ── Injections ─────────────────────────────────────────
        [Inject] private EmployeService EmployeService { get; set; } = default!;
        [Inject] private NavigationManager Navigation    { get; set; } = default!;
        [Inject] private IJSRuntime JS                   { get; set; } = default!;

        // ── Paramètre URL : /employe/equipement/{AffectationId} ──
        [Parameter] public int AffectationId { get; set; }

        // ── Données ────────────────────────────────────────────
        // EquipementAffecteDto est défini dans Services/EmployeService.cs
        private EquipementAffecteDto? Equipement { get; set; }
        private bool IsLoading { get; set; } = true;

        // ── Infos utilisateur (depuis localStorage via EmployeService) ──
        private string UserName { get; set; } = "Utilisateur";
        private string UserRole { get; set; } = "Employé";

        // ── QR Code ────────────────────────────────────────────
        // URL encodée dans le QR : page publique lisible sur mobile
        // La page /fiche/{id} ne nécessite PAS d'authentification
        private string FicheUrl => $"{Navigation.BaseUri}fiche/{AffectationId}";
        private string QrSvg    { get; set; } = string.Empty;

        // ── Formulaire incident ────────────────────────────────
        private bool   ShowForm           { get; set; } = false;
        private bool   IncidentEnvoye     { get; set; } = false;
        private bool   EnCours            { get; set; } = false;
        private string IncidentType        { get; set; } = string.Empty;
        private string IncidentDescription { get; set; } = string.Empty;
        private string ErreurForm          { get; set; } = string.Empty;

        // ── Init ───────────────────────────────────────────────
        protected override async Task OnInitializedAsync()
        {
            // Infos utilisateur depuis localStorage
            UserName = await EmployeService.GetCurrentUserNameAsync();
            UserRole = await EmployeService.GetCurrentUserRoleAsync();

            // Chargement équipement
            await ChargerEquipement();
        }

        protected override void OnParametersSet()
        {
            // Génère le QR dès que AffectationId est connu
            // (FicheUrl dépend de Navigation.BaseUri disponible côté client)
            QrSvg = BuildQrSvg(FicheUrl);
        }

        // ── Chargement ─────────────────────────────────────────

        private async Task ChargerEquipement()
        {
            IsLoading = true;
            try
            {
                // Utilise GetEquipementDetailAsync(int affectationId)
                // définie dans Services/EmployeService.cs
                Equipement = await EmployeService.GetEquipementDetailAsync(AffectationId);
            }
            catch
            {
                Equipement = null;
            }
            finally
            {
                IsLoading = false;
            }
        }

        // ── Incident ───────────────────────────────────────────

        private void Annuler()
        {
            ShowForm           = false;
            IncidentType        = string.Empty;
            IncidentDescription = string.Empty;
            ErreurForm          = string.Empty;
        }

        private async Task Soumettre()
        {
            ErreurForm = string.Empty;

            if (string.IsNullOrWhiteSpace(IncidentType))
            {
                ErreurForm = "Veuillez sélectionner un type d'incident.";
                return;
            }
            if (string.IsNullOrWhiteSpace(IncidentDescription))
            {
                ErreurForm = "Veuillez décrire le problème.";
                return;
            }

            EnCours = true;
            try
            {
                // SignalerIncidentDto est défini dans Services/EmployeService.cs
                var dto = new SignalerIncidentDto
                {
                    AffectationId = AffectationId,
                    TypeIncident  = IncidentType,
                    Description   = IncidentDescription,
                    DateIncident  = DateTime.UtcNow
                };

                // SignalerIncidentAsync retourne (bool Success, string Message)
                var (success, message) = await EmployeService.SignalerIncidentAsync(dto);

                if (success)
                {
                    IncidentEnvoye = true;
                    ShowForm       = false;
                    // Recharge pour mettre à jour le statut si changé (Perdu/Endommage)
                    await ChargerEquipement();
                }
                else
                {
                    ErreurForm = message;
                }
            }
            finally
            {
                EnCours = false;
            }
        }

        // ── Impression QR ──────────────────────────────────────

        /// <summary>
        /// Ouvre une mini-page d'impression contenant le QR Code.
        /// Aucune dépendance externe : le SVG est généré en C#.
        /// </summary>
        private async Task ImprimerQR()
        {
            var designation = Equipement?.Designation ?? "Équipement";
            var reference   = Equipement?.Reference   ?? "";

            // HTML minimal pour la fenêtre d'impression
            var printHtml = $@"<!DOCTYPE html>
<html lang=""fr"">
<head>
  <meta charset=""utf-8""/>
  <title>QR — {designation}</title>
  <style>
    body {{
      font-family: sans-serif;
      display: flex;
      flex-direction: column;
      align-items: center;
      padding: 2rem;
      background: white;
      color: #111;
    }}
    h2  {{ font-size: 1.2rem; font-weight: 800; margin: 1rem 0 0.25rem; }}
    p   {{ font-size: 0.8rem; color: #555; margin: 0; }}
    code{{ font-size: 0.65rem; color: #333; margin-top: 0.75rem; display: block; }}
    @media print {{ body {{ padding: 0; }} }}
  </style>
</head>
<body>
  {QrSvg}
  <h2>{designation}</h2>
  <p>Référence : {reference}</p>
  <code>{FicheUrl}</code>
  <script>window.onload = () => window.print();<\/script>
</body>
</html>";

            await JS.InvokeVoidAsync("eval", $@"
                var w = window.open('','_blank','width=400,height=500');
                w.document.write({System.Text.Json.JsonSerializer.Serialize(printHtml)});
                w.document.close();
            ");
        }

        // ── Helpers ────────────────────────────────────────────

        private string GetInitials()
        {
            var parts = UserName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2) return $"{parts[0][0]}{parts[1][0]}".ToUpper();
            if (parts.Length == 1 && parts[0].Length >= 2) return parts[0][..2].ToUpper();
            return "??";
        }

        private string GetStatutLabel(string statut) => statut switch
        {
            "EnCours"   => "En Service",
            "Retourne"  => "Retourné",
            "Perdu"     => "Perdu",
            "Endommage" => "Endommagé",
            _           => statut
        };

        // ── Génération QR Code SVG ─────────────────────────────
        private string BuildQrSvg(string url)
        {
            const int Size   = 25;   // version 2 : 25×25 modules
            const int CellPx = 8;
            const int Margin  = 16;

            var grid = new bool[Size, Size];

            // ── 1. Finder patterns (carré 7×7) dans 3 coins ──
            PlaceFinder(grid, 0,      0,      Size);   // haut-gauche
            PlaceFinder(grid, Size-7, 0,      Size);   // bas-gauche
            PlaceFinder(grid, 0,      Size-7, Size);   // haut-droite

            // ── 2. Timing patterns (ligne 6 et colonne 6) ──
            for (int i = 8; i < Size - 8; i++)
            {
                grid[6, i] = (i % 2 == 0);
                grid[i, 6] = (i % 2 == 0);
            }

            // ── 3. Dark module (obligatoire en version 2) ──
            if (Size > 8) grid[Size - 8, 8] = true;

            // ── 4. Encodage URL → bits ──
            // Byte-mode : length byte + données ASCII
            var bytes = new List<byte>();
            bytes.Add((byte)url.Length);
            foreach (var c in url)
                bytes.Add((byte)(c < 128 ? c : '?'));

            // Convertit les bytes en bits
            var bits = new List<bool>();
            foreach (var b in bytes)
                for (int k = 7; k >= 0; k--)
                    bits.Add((b >> k & 1) == 1);

            // ── 5. Remplit les modules de données ──
            // Parcourt la grille en colonnes paires de droite à gauche
            // en évitant les zones réservées
            int bitIndex = 0;
            bool goingUp = true;

            for (int col = Size - 1; col >= 0; col -= 2)
            {
                if (col == 6) col--;  // saute la colonne timing
                for (int rowStep = 0; rowStep < Size; rowStep++)
                {
                    int row = goingUp ? (Size - 1 - rowStep) : rowStep;
                    for (int cx = 0; cx <= 1; cx++)
                    {
                        int c = col - cx;
                        if (c < 0 || grid[row, c]) continue;  // déjà occupé
                        if (bitIndex < bits.Count)
                        {
                            grid[row, c] = bits[bitIndex++];
                        }
                        else
                        {
                            // Padding : alternance pour masque 0
                            grid[row, c] = (row + c) % 2 == 0;
                        }
                    }
                }
                goingUp = !goingUp;
            }

            // ── 6. Rendu SVG ──
            int svgSize = Size * CellPx + Margin * 2;
            var sb = new StringBuilder();

            sb.Append($"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{svgSize}\" height=\"{svgSize}\" viewBox=\"0 0 {svgSize} {svgSize}\" class=\"qr-svg\">");
            sb.Append($"<rect width=\"{svgSize}\" height=\"{svgSize}\" fill=\"white\"/>");

            for (int r = 0; r < Size; r++)
                for (int c2 = 0; c2 < Size; c2++)
                    if (grid[r, c2])
                        sb.Append($"<rect x=\"{c2 * CellPx + Margin}\" y=\"{r * CellPx + Margin}\" width=\"{CellPx}\" height=\"{CellPx}\" fill=\"#0F1E3C\"/>");

            sb.Append("</svg>");
            return sb.ToString();
        }

        /// <summary>
        /// Place un finder pattern 7×7 (bordure + centre) avec séparateur.
        /// </summary>
        private static void PlaceFinder(bool[,] g, int row, int col, int size)
        {
            for (int r = 0; r < 7; r++)
                for (int c = 0; c < 7; c++)
                {
                    if (row + r >= size || col + c >= size) continue;
                    bool border = r == 0 || r == 6 || c == 0 || c == 6;
                    bool center = r >= 2 && r <= 4 && c >= 2 && c <= 4;
                    g[row + r, col + c] = border || center;
                }

            // Séparateur blanc autour du finder
            for (int r = -1; r <= 7; r++)
                for (int c = -1; c <= 7; c++)
                {
                    int rr = row + r, cc = col + c;
                    if (rr < 0 || rr >= size || cc < 0 || cc >= size) continue;
                    if (r == -1 || r == 7 || c == -1 || c == 7)
                        g[rr, cc] = false;
                }
        }
    }
}