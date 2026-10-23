# Artwork provenance

Updated 2026-10-23.

## Artwork used by the current build

- `Assets/Artwork/rotor-toaster.png`: new turquoise angular toaster with an orange propeller, four animation frames.
- `Assets/Artwork/golden-toast.png`: newly generated stylized toast slice, revised to a horizontal isometric pose
  with a visible crust sidewall. The legacy toast was viewed to assess orientation only; its pixels were not
  supplied as generation input or included in the new asset.
- `Assets/Artwork/breadbox-computer.png`: newly drawn recognizable C64 breadbin depiction with a model badge. Its red lamp is animated by the application.
- `Assets/Artwork/wedge-computer.png`: newly drawn recognizable Amiga 500-style computer with a model wordmark.
- `Assets/Artwork/floppy-disk.png`: newly generated disk, composed with the wedge computer for the insertion/ejection animation.
- `Assets/logo.gif` and `Assets/logogs.png`: existing project-owner-supplied computer and company logos.
- `Assets/app-icon.png/.ico/.icns`: technical conversions of the owner's `logo.gif`, as requested.

The five new sprite assets were created using the built-in image_gen tool.
The initial generations used text prompts only, with no original After Dark sprites or other reference images.
Follow-up edits changed the indicator and then revised both computers toward recognizable historical hardware,
using only the previously generated project sprites as image inputs. Hardware names and badges identify
the depicted machines; no permission or manufacturer endorsement is claimed.
The application uses a fixed toaster body with a continuously rotated propeller, a small shared
two-state texture for the flashing red light, and a separate disk texture with continuous sliding motion.

Prompts and generation details are retained in [artwork-prompts.json](artwork-prompts.json).
These are independently generated stylizations, not traced copies of the old sprite files.
This creation record is not a guarantee of exclusivity or a formal trademark/design clearance.

## Legacy artwork

The project owner described the former `toaster-sprite.gif` and `toast1.gif` as reconstructions of the original.
Those files are no longer in the application's asset directory or build resources.
Local backups are preserved under the ignored `artifacts/legacy-artwork` directory.
Existing Git history was not rewritten and may still contain the earlier files.

The earlier review found no verified permission for reproducing the original character design.
Relevant background remains:

- [After Dark in CSS: license section](https://github.com/bryanbraun/after-dark-css#license) separates MIT code from artwork attributed to Berkeley Systems. It is not an established source for this project's former files.
- [German Copyright Act, section 23](https://www.gesetze-im-internet.de/urhg/__23.html) addresses adaptations and sufficient distance from an existing work.
- [German Trademark Act, section 14](https://www.gesetze-im-internet.de/markeng/__14.html) addresses protected signs used in trade.

C64 and Amiga names identify the historical inspirations. This independent project does not imply endorsement by hardware manufacturers or their successors.
