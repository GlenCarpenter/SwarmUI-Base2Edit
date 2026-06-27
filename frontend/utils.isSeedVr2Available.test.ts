import { describe, expect, it } from "@jest/globals";
import { utils } from "./utils";

describe("utils.isSeedVr2Available", () => {
    it("returns false when no SeedVR2 elements are present", () => {
        expect(utils.isSeedVr2Available()).toBe(false);
    });

    it("returns true when the SeedVR2 model param input is present", () => {
        const el = document.createElement("input");
        el.id = "input_seedvrmodel";
        document.body.appendChild(el);

        expect(utils.isSeedVr2Available()).toBe(true);
    });

    it("returns true when the SeedVR2 group container is present", () => {
        const el = document.createElement("div");
        el.id = "auto-group-seedvrupscaler";
        document.body.appendChild(el);

        expect(utils.isSeedVr2Available()).toBe(true);
    });

    it("does NOT match the digit-preserving id input_seedvr2model (locks the CleanTypeName contract)", () => {
        const el = document.createElement("input");
        el.id = "input_seedvr2model";
        document.body.appendChild(el);

        expect(utils.isSeedVr2Available()).toBe(false);
    });

    it("does NOT match the digit-preserving group id auto-group-seedvr2upscaler", () => {
        const el = document.createElement("div");
        el.id = "auto-group-seedvr2upscaler";
        document.body.appendChild(el);

        expect(utils.isSeedVr2Available()).toBe(false);
    });
});
